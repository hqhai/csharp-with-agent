// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ClassForumCmd.V1i2
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.ClassForumResults.V1i2;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.StorageServices;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.ApplicationServices.CacheServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Refit;

    public class CreateCFRPendingWordContentCommand : CreateCFRPendingWordContentCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateCFRPendingWordContentCommandHandler
        : IRequestHandler<CreateCFRPendingWordContentCommand, MethodResult<bool>>
    {
        private readonly IUserService _userService;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IClassForumDetailResultRepository _classForumDetailResultRepository;
        private readonly IClassForumResultFileRepository _classForumResultFileRepository;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly AuthContext _authContext;
        private readonly ISystemService _systemService;
        private readonly SpeechToTextPendingAiPublisher _speechToTextPendingAiPublisher;
        private readonly IStorageService _storageService;
        private readonly ILogger<CreateCFRPendingWordContentCommand> _logger;
        private readonly IRequestSafeCachingService _requestSafeCachingService;

        private const int MaxClassForumDetailResultRecord = 2;
        private const int MaxPendingSpeechToText = 2;
        private const int TimeStartJobTest = 10; // minutes
        private const int TimeStartJobProd = 2;  // hours

        public CreateCFRPendingWordContentCommandHandler(
            IUserService userService,
            IClassForumResultRepository classForumResultRepository,
            IClassForumDetailResultRepository classForumDetailResultRepository,
            IClassForumResultFileRepository classForumResultFileRepository,
            IHostEnvironment hostEnvironment,
            AuthContext authContext,
            ISystemService systemService,
            SpeechToTextPendingAiPublisher speechToTextPendingAiPublisher,
            IStorageService storageService,
            ILogger<CreateCFRPendingWordContentCommand> logger,
            IRequestSafeCachingService requestSafeCachingService)
        {
            _userService = userService;
            _classForumResultRepository = classForumResultRepository;
            _classForumDetailResultRepository = classForumDetailResultRepository;
            _classForumResultFileRepository = classForumResultFileRepository;
            _hostEnvironment = hostEnvironment;
            _authContext = authContext;
            _systemService = systemService;
            _speechToTextPendingAiPublisher = speechToTextPendingAiPublisher;
            _storageService = storageService;
            _logger = logger;
            _requestSafeCachingService = requestSafeCachingService;
        }

        public async Task<MethodResult<bool>> Handle(CreateCFRPendingWordContentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.FormFile);

            var result = new MethodResult<bool>();

            // ===== Base64 text check =====
            if (StringHelper.IsBase64Image(request.Content))
            {
                result.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.Base64InText));
                return result;
            }

            // ===== Forbidden words =====
            var forbiddenResult = await _systemService
                .CheckContainForbiddenWord(request.Content ?? string.Empty);

            var forbiddenWords = (forbiddenResult.Content?.Result ?? Enumerable.Empty<string>())
                .Distinct()
                .ToList();

            if (forbiddenWords.Any())
            {
                result.AddErrorBadRequest(
                    nameof(EnumClassForumResultErrorCode.ContainsForbiddenKeywords),
                    string.Join(", ", forbiddenWords));
                return result;
            }

            // ===== Load ClassForumResult by ID =====
            var classForumResult = await _classForumResultRepository.Queryable
                                                                    .Include(x => x.ClassForumDetailResults.OrderBy(d => d.CreatedDate))
                                                                    .FirstOrDefaultAsync(x => x.Id == request.ClassForumResultId, cancellationToken);
            if (classForumResult == null)
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(ClassForumResult));
                return result;
            }

            // ===== Max pending STT =====
            var pendingCount = await _classForumResultRepository.Queryable
                                                                .Where(x => x.StudentId == classForumResult.StudentId && x.IsPendingSpeechToText)
                                                                .CountAsync(cancellationToken);

            if (pendingCount >= MaxPendingSpeechToText)
            {
                result.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.MaxPendingSpeechToText));
                return result;
            }

            // ===== Limit detail count =====
            if (classForumResult.ClassForumDetailResults.Count >= MaxClassForumDetailResultRecord)
            {
                result.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.ClassForumDetailHaveMoreThan2));
                return result;
            }

            // ===== Block if pending =====
            if (classForumResult.ClassForumDetailResults
                .Any(x => x.Status == EnumClassForumResultStatus.PendingSpeechToText))
            {
                result.AddErrorBadRequest(
                    nameof(EnumClassForumResultErrorCode.AIPendingRecord1Record2CreationBlocked));
                return result;
            }

            // ===== Time up check =====
            var firstAttempt = classForumResult.ClassForumDetailResults.FirstOrDefault();
            if (firstAttempt?.ProcessDate.HasValue == true)
            {
                var deadline = (_hostEnvironment.IsDevelopment()
                                || _hostEnvironment.IsEnvironment(Settings.Environments.Testing))
                    ? firstAttempt.ProcessDate.Value.AddMinutes(TimeStartJobTest)
                    : firstAttempt.ProcessDate.Value.AddHours(TimeStartJobProd);

                if (deadline <= DateTime.UtcNow)
                {
                    result.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.TimeUpPostClassForum));
                    return result;
                }
            }

            ClassForumDetailResult? detailResult = null;

            await _classForumResultRepository.ExecuteTransactionAsync(async () =>
            {
                var submissionCount = classForumResult.ClassForumDetailResults.Count == 0
                    ? EnumSubmissionCount.FirstSubmit
                    : EnumSubmissionCount.SecondSubmit;

                detailResult = await CreateClassForumDetailResultAsync(
                    classForumResult.Id,
                    submissionCount,
                    request.Content ?? string.Empty,
                    request.FormFile);
                classForumResult.ProcessDate = DateTime.UtcNow;
                classForumResult.ResultStatus = EnumResultStatus.Done;
                classForumResult.IsPendingSpeechToText = true;
                await _classForumResultRepository.BulkUpdateList(new List<ClassForumResult> { classForumResult },
                bulk => bulk.IgnoreOnUpdateExpression = e => new
                {
                    e.ClassForumId,
                    e.LessonResultId,
                    e.StudentId
                });
                await _classForumResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                result.Result = true;
                result.StatusCode = StatusCodes.Status200OK;
                return result;
            });

            // ===== Publish STT =====
            using var ms = new MemoryStream();
            await request.FormFile.CopyToAsync(ms, cancellationToken);

            await _speechToTextPendingAiPublisher.Publish(
                new SpeechToTextPendingAiConsumerModel
                {
                    UserId = _authContext.CurrentUserId,
                    ClassForumDetailResultId = detailResult.Id,
                    FileName = request.FormFile.FileName,
                    ContentType = request.FormFile.ContentType,
                    FileData = ms.ToArray()
                },
                cancellationToken);

            _logger.LogInformation(
                "Pending STT published | User={UserId} | DetailId={DetailId} | File={FileName} | Size={Size}",
                _authContext.CurrentUserId,
                detailResult.Id,
                request.FormFile.FileName,
                ms.Length);

            return result;
        }

        private async Task<ClassForumDetailResult> CreateClassForumDetailResultAsync(
            Guid classForumResultId,
            EnumSubmissionCount submissionCount,
            string content,
            IFormFile formFile)
        {
            string? filePath;

            using var stream = formFile.OpenReadStream();
            var streamPart = new StreamPart(stream, formFile.FileName, formFile.ContentType);

            var wavResult = await _storageService.ConvertWav(streamPart);
            if (wavResult.IsSuccessStatusCode)
            {
                filePath = wavResult.Content?.Result;
            }
            else
            {
                using var s3Stream = formFile.OpenReadStream();
                var s3Part = new StreamPart(s3Stream, formFile.FileName, formFile.ContentType);
                var uploadResult = await _storageService.UpLoadFile(
                    EnumFolderType.Files,
                    EnumBucketType.FselPublic,
                    s3Part);
                filePath = uploadResult.Content?.Result;
            }

            var detail = new ClassForumDetailResult
            {
                Content = content,
                Status = EnumClassForumResultStatus.PendingSpeechToText,
                SubmissionCount = submissionCount,
                ClassForumResultId = classForumResultId,
                ProcessDate = submissionCount == EnumSubmissionCount.FirstSubmit ? DateTime.UtcNow : null,
                ClassForumResultFiles = new List<ClassForumResultFile>
                {
                    new ClassForumResultFile { FilePath = filePath }
                }
            };

            await _requestSafeCachingService.SafeRequest<ClassForumDetailResult>(
                key: $"Add_ClassForumDetailResult_{detail.SubmissionCount}_{detail.ClassForumResultId}_{detail.IsDeleted}",
                safeFunction: async () =>
                {
                    await _classForumDetailResultRepository.BulkMergeAsync(
                        new List<ClassForumDetailResult> { detail },
                        bulk =>
                        {
                            bulk.ColumnPrimaryKeyExpression = x => new { x.SubmissionCount, x.ClassForumResultId, x.IsDeleted };
                        });
                    return detail;
                });

            foreach (var f in detail.ClassForumResultFiles)
            {
                f.ClassForumDetailResultId = detail.Id;
            }

            if (detail.ClassForumResultFiles.Any())
            {
                var classForumResultFiles = detail.ClassForumResultFiles.ToList();
                await _requestSafeCachingService.SafeRequest(
                    key: $"Add_ClassForumResultFiles_{string.Join("_", classForumResultFiles.Select(hwa => $"{hwa.ClassForumDetailResultId}_{hwa.FilePath}"))}",
                    safeFunction: async () =>
                    {
                        await _classForumResultFileRepository.BulkMergeAsync(classForumResultFiles);
                        return classForumResultFiles;
                    });
            }

            return detail;
        }
    }
}
