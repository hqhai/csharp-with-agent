// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ClassForumCmd.V1i1
{
    using System.IO;
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
    using Fsel.Course.Domain.Models.CommandModels.ClassForumResults.V1i1;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.StorageServices;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Refit;
    using Microsoft.Extensions.Logging;
    using Fsel.Course.Infrastructure.Repositories;

    public class CreateCFRPendingWordContentCommand : CreateCFRPendingWordContentCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateCFRPendingWordContentCommandHandler : IRequestHandler<CreateCFRPendingWordContentCommand, MethodResult<bool>>
    {
        private readonly IUserService _userService;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IClassForumDetailResultRepository _classForumDetailResultRepository;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly AuthContext _authContext;
        private readonly ISystemService _systemService;
        private readonly SpeechToTextPendingAiPublisher _speechToTextPendingAiPublisher;
        private readonly IStorageService _storageService;
        private readonly IMediator _mediator;
        private readonly ILogger<CreateCFRPendingWordContentCommand> _logger;
        private readonly IClassForumResultFileRepository _classForumResultFileRepository;
        private const int MaxClassForumDetailResultRecord = 2;
        private const int MaxPendingSpeechToText = 2;
        private const int TimeStartJobTest = 10;
        private const int TimeStartJobProd = 2;

        public CreateCFRPendingWordContentCommandHandler(IUserService userService,
                                                         IClassForumResultRepository classForumResultRepository,
                                                         IClassForumRepository classForumRepository,
                                                         ILessonResultRepository lessonResultRepository,
                                                         IClassForumDetailResultRepository classForumDetailResultRepository,
                                                         IHostEnvironment hostEnvironment,
                                                         AuthContext authContext,
                                                         ISystemService systemService,
                                                         SpeechToTextPendingAiPublisher speechToTextPendingAiPublisher,
                                                         IStorageService storageService,
                                                         IMediator mediator,
                                                        ILogger<CreateCFRPendingWordContentCommand> logger,
                                                        IClassForumResultFileRepository classForumResultFileRepository)
        {
            _userService = userService;
            _classForumResultRepository = classForumResultRepository;
            _classForumRepository = classForumRepository;
            _lessonResultRepository = lessonResultRepository;
            _classForumDetailResultRepository = classForumDetailResultRepository;
            _hostEnvironment = hostEnvironment;
            _authContext = authContext;
            _systemService = systemService;
            _speechToTextPendingAiPublisher = speechToTextPendingAiPublisher;
            _storageService = storageService;
            _mediator = mediator;
            _logger = logger;
            _classForumResultFileRepository = classForumResultFileRepository;
        }

        public async Task<MethodResult<bool>> Handle(CreateCFRPendingWordContentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.FormFile);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            if (StringHelper.IsBase64Image(request.Content))
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.Base64InText));
                return methodResult;
            }

            // Check từ khoá cấm
            var listForbiddenWordResultContent = await _systemService.CheckContainForbiddenWord(request.Content ?? string.Empty);
            var containsForbiddenWord = (listForbiddenWordResultContent.Content?.Result ?? Enumerable.Empty<string>()).Distinct().ToList();
            if (containsForbiddenWord.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.ContainsForbiddenKeywords), string.Join(", ", containsForbiddenWord));
                return methodResult;
            }

            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult.Content?.Result;

            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var studentId = student.Id;

            // check đã quá số lần tối đa được phép pending speech to text
            var pendingSpeechToText = await _classForumResultRepository.Queryable
                                                                       .Where(x => x.StudentId == studentId && x.IsPendingSpeechToText)
                                                                       .CountAsync(cancellationToken);
            if (pendingSpeechToText >= MaxPendingSpeechToText)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.MaxPendingSpeechToText));
                return methodResult;
            }

            var lessonResult = await _lessonResultRepository.GetByIdAsync(request.LessonResultId);
            if (lessonResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lessonResult));
                return methodResult;
            }

            var classForum = await _classForumRepository.Queryable.FirstOrDefaultAsync(x => x.LessonId == lessonResult.LessonId, cancellationToken);
            if (classForum == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForum));
                return methodResult;
            }

            var classForumResult = await _classForumResultRepository.Queryable
                                                                    .Include(x => x.ClassForumDetailResults.OrderBy(x => x.CreatedDate))
                                                                    .FirstOrDefaultAsync(x => x.StudentId == studentId && x.LessonResultId == request.LessonResultId, cancellationToken);

            // check đã quá 2 bản ghi ClassForumDetailResults
            if (classForumResult != null && classForumResult.ClassForumDetailResults.Count >= MaxClassForumDetailResultRecord)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.ClassForumDetailHaveMoreThan2));
                return methodResult;
            }

            // check bản ghi 1 AI chưa trả về thì bản ghi 2 không được tạo
            if (classForumResult != null && classForumResult.ClassForumDetailResults.Any(x => x.Status == EnumClassForumResultStatus.PendingSpeechToText))
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.AIPendingRecord1Record2CreationBlocked));
                return methodResult;
            }

            // check nếu đã hết thời gian up file thứ 2 = thời gian tổng hợp job (chạy từ lúc bản ghi 1 có phản hồi AI)
            var classForumDetailResultAttemp1 = classForumResult?.ClassForumDetailResults.FirstOrDefault();
            if (classForumDetailResultAttemp1 != null && classForumDetailResultAttemp1.ProcessDate.HasValue)
            {
                DateTime processDateResultAttemp1 = ((_hostEnvironment.IsDevelopment() || _hostEnvironment.IsEnvironment(Settings.Environments.Testing)) ?
                                                    (classForumDetailResultAttemp1.ProcessDate.Value.AddMinutes(TimeStartJobTest)) :
                                                    (classForumDetailResultAttemp1.ProcessDate.Value.AddHours(TimeStartJobProd)));

                if (processDateResultAttemp1 <= DateTime.UtcNow)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.TimeUpPostClassForum), nameof(classForum));
                    return methodResult;
                }
            }

            ClassForumDetailResult classForumDetailResult = new ClassForumDetailResult();
            await _classForumResultRepository.ExecuteTransactionAsync(async () =>
            {
                if (classForumResult == null)
                {
                    var newClassForumResult = await CreateClassForumResultAsync(request.LessonResultId, classForum.Id, studentId, cancellationToken);
                    classForumDetailResult = await CreateClassForumDetailResultAsync(newClassForumResult.Id, EnumSubmissionCount.FirstSubmit, request.Content ?? string.Empty, request.FormFile, cancellationToken);
                }
                else
                {
                    classForumDetailResult = await CreateClassForumDetailResultAsync(classForumResult.Id, EnumSubmissionCount.SecondSubmit, request.Content ?? string.Empty, request.FormFile, cancellationToken);
                    await UpdateClassForumResult(classForumResult);
                }

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });

            // bắn publish sang xử lý speech to text
            using var memoryStream = new MemoryStream();
            await request.FormFile.CopyToAsync(memoryStream, cancellationToken);

            await _speechToTextPendingAiPublisher.Publish(new SpeechToTextPendingAiConsumerModel
            {
                UserId = _authContext.CurrentUserId,
                ClassForumDetailResultId = classForumDetailResult.Id,
                FileName = request.FormFile.FileName,
                ContentType = request.FormFile.ContentType,
                FileData = memoryStream.ToArray()
            }, cancellationToken);

            _logger.LogError($"LogParamPendingSTT: userId: {_authContext.CurrentUserId} classForumDetailResult: {classForumDetailResult.Id} value: {request.FormFile.FileName} - {request.FormFile.ContentType} - {memoryStream.ToArray()}");

            return methodResult;
        }

        private async Task<ClassForumResult> CreateClassForumResultAsync(Guid lessonResultId, Guid classForumId, Guid studentId, CancellationToken cancellationToken)
        {
            var classForumResult = new ClassForumResult
            {
                StudentId = studentId,
                LessonResultId = lessonResultId,
                ClassForumId = classForumId,
                SubmissionCount = EnumSubmissionCount.FirstSubmit,
                IsPendingSpeechToText = true
            };

            await _classForumResultRepository.BulkMergeAsync(new List<ClassForumResult> { classForumResult }, bulk =>
            {
                bulk.ColumnPrimaryKeyExpression = c => new { c.LessonResultId, c.ClassForumId, c.StudentId, c.IsDeleted };
            });
            await _classForumResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            return classForumResult;
        }

        private async Task<ClassForumDetailResult> CreateClassForumDetailResultAsync(Guid classForumResultId, EnumSubmissionCount submissionCount, string content, IFormFile formFile, CancellationToken cancellationToken)
        {
            string? filePath = null;
            using var stream = formFile.OpenReadStream();
            var streamPart = new StreamPart(stream, formFile.FileName, formFile.ContentType);

            var filePart = await _storageService.ConvertWav(streamPart);
            if (!filePart.IsSuccessStatusCode)
            {
                using var streamS3 = formFile.OpenReadStream();
                var streamPartS3 = new StreamPart(streamS3, formFile.FileName, formFile.ContentType);
                var filePartS3 = await _storageService.UpLoadFile(EnumFolderType.Files, EnumBucketType.FselPublic, streamPartS3);
                filePath = filePartS3.Content?.Result;
            }
            else
            {
                filePath = filePart.Content?.Result;
            }

            var classForumDetailResult = new ClassForumDetailResult
            {
                Content = content,
                Status = EnumClassForumResultStatus.PendingSpeechToText,
                SubmissionCount = submissionCount,
                ClassForumResultId = classForumResultId,
                ClassForumResultFiles = new List<ClassForumResultFile> { new ClassForumResultFile { FilePath = filePath } }
            };

            await _classForumDetailResultRepository.BulkMergeAsync(new List<ClassForumDetailResult> { classForumDetailResult }, bulk =>
            {
                bulk.ColumnPrimaryKeyExpression = c => new { c.SubmissionCount, c.ClassForumResultId, c.IsDeleted };
            });
            if (classForumDetailResult.ClassForumResultFiles.Any())
            {
                foreach (var file in classForumDetailResult.ClassForumResultFiles)
                {
                    file.ClassForumDetailResultId = classForumDetailResult.Id; // Set foreign key nếu cần
                }
                await _classForumResultFileRepository.BulkMergeAsync(classForumDetailResult.ClassForumResultFiles);
            }
            return classForumDetailResult;
        }

        private async Task UpdateClassForumResult(ClassForumResult classForumResult)
        {
            classForumResult.IsPendingSpeechToText = true;
            await _classForumResultRepository.BulkUpdateList(new List<ClassForumResult> { classForumResult }, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = entity => new { entity.ClassForumId, entity.LessonResultId, entity.StudentId };
            });
        }
    }
}
