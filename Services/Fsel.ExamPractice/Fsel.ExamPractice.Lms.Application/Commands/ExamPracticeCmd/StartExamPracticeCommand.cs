// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Commands.ExamPracticeCmd
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Entities.Configs;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.Enums.ErrorCodes;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
    using Fsel.ExamPractice.Lms.Application.Services.UserServices;
    using Fsel.ExamPractice.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class StartExamPracticeCommand : IRequest<MethodResult<ExamPracticeResultModel>>
    {
        public Guid Id { get; set; }
        public EnumPracticeMode PracticeMode { get; set; }
        public IList<Guid>? ExamPracticeSectionIds { get; set; }
        public bool IsAllPart { get; set; }
        public EnumPracticeTimeLimitOption? PracticeTimeLimitOption { get; set; }
    }

    public class StartExamPracticeCommandHandler : IRequestHandler<StartExamPracticeCommand, MethodResult<ExamPracticeResultModel>>
    {
        private readonly IExamPracticeResultRepository _examPracticeResultRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly IExamPracticeRepository _examPracticeRepository;
        private readonly IExamPracticeSectionRepository _examPracticeSectionRepository;
        private readonly IExamPracticeRetryRepository _examPracticeRetryRepository;
        private const int MaxRetryAttempts = 20;

        public StartExamPracticeCommandHandler(IExamPracticeResultRepository examPracticeResultRepository,
            IMapper mapper,
            IUserService userService,
            AuthContext authContext,
            IExamPracticeRepository examPracticeRepository,
            IExamPracticeSectionRepository examPracticeSectionRepository,
            IExamPracticeRetryRepository examPracticeRetryRepository)
        {
            _examPracticeResultRepository = examPracticeResultRepository;
            _mapper = mapper;
            _userService = userService;
            _authContext = authContext;
            _examPracticeRepository = examPracticeRepository;
            _examPracticeSectionRepository = examPracticeSectionRepository;
            _examPracticeRetryRepository = examPracticeRetryRepository;
        }

        public async Task<MethodResult<ExamPracticeResultModel>> Handle(StartExamPracticeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ExamPracticeResultModel>();

            var student = await GetStudentAsync(methodResult, cancellationToken);
            if (student == null)
            {
                return methodResult;
            }

            var examPractice = await GetExamPracticeAsync(request.Id, methodResult, cancellationToken);
            if (examPractice == null)
            {
                return methodResult;
            }
            if (!ValidateExamPracticeStatus(examPractice, methodResult))
            {
                return methodResult;
            }
            if (!ValidateFullMockTest(request, examPractice, methodResult))
            {
                return methodResult;
            }
            if (!await ValidateSectionIdsAsync(request, methodResult))
            {
                return methodResult;
            }
            if (!ValidateSkillMockTest(request, examPractice, methodResult))
            {
                return methodResult;
            }
            if (!ValidateExamPracticeType(request, examPractice, methodResult))
            {
                return methodResult;
            }

            var examPracticeRetry = await GetOrCreateExamPracticeRetryAsync(student.Id, examPractice.Id, methodResult, cancellationToken);
            if (examPracticeRetry == null)
            {
                return methodResult;
            }
            var examPracticeResult = await _examPracticeResultRepository.Queryable
                                            .Where(x => x.ExamPracticeRetryId == examPracticeRetry.Id && x.WorkingStatus == EnumWorkingStatus.Active)
                                            .FirstOrDefaultAsync(cancellationToken);

            if (!ValidateExamPracticeResult(examPracticeResult, methodResult))
            {
                return methodResult;
            }
            examPracticeResult = await CreateExamPracticeResultAsync(request, student.Id, examPractice, examPracticeRetry, cancellationToken);

            await _examPracticeResultRepository.ExecuteTransactionAsync(async () =>
            {
                await _examPracticeResultRepository.BulkMergeAsync(new List<ExamPracticeResult> { examPracticeResult }, bulk =>
                {
                    bulk.ColumnPrimaryKeyExpression = entity => new { entity.StudentId, entity.ExamPracticeId, entity.ExamPracticeRetryId, entity.WorkingStatus };
                });
                return methodResult;
            });

            methodResult.Result = _mapper.Map<ExamPracticeResultModel>(examPracticeResult);
            return methodResult;
        }

        private async Task<StudentModel?> GetStudentAsync(MethodResult<ExamPracticeResultModel> methodResult, CancellationToken cancellationToken)
        {
            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return null;
            }
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return null;
            }
            return student;
        }

        private async Task<ExamPractice?> GetExamPracticeAsync(Guid id, MethodResult<ExamPracticeResultModel> methodResult, CancellationToken cancellationToken)
        {
            var examPractice = await _examPracticeRepository.Queryable.AsNoTracking().Include(x => x.ExamPracticeSections).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (examPractice == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPractice), id);
                return null;
            }
            return examPractice;
        }

        private static bool ValidateExamPracticeStatus(ExamPractice examPractice, MethodResult<ExamPracticeResultModel> methodResult)
        {
            if (examPractice.Status != EnumExamPracticeStatus.Active)
            {
                methodResult.AddErrorBadRequest(nameof(EnumExamPracticeErrorCode.TestStatusUpdated), nameof(examPractice.Status), examPractice.Status);
                return false;
            }
            return true;
        }

        private static bool ValidateFullMockTest(StartExamPracticeCommand request, ExamPractice examPractice, MethodResult<ExamPracticeResultModel> methodResult)
        {
            if (examPractice.SubType == EnumExamPracticeSubType.FullMockTest)
            {
                if (request.PracticeMode == EnumPracticeMode.Practice)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(EnumExamPracticeSubType.FullMockTest), request.PracticeMode);
                    return false;
                }
                if (request.ExamPracticeSectionIds != null && request.ExamPracticeSectionIds.Any())
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(EnumExamPracticeSubType.FullMockTest), request.ExamPracticeSectionIds);
                    return false;
                }
                if (request.PracticeTimeLimitOption.HasValue)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(EnumExamPracticeSubType.FullMockTest), request.PracticeTimeLimitOption);
                    return false;
                }
            }
            return true;
        }

        private async Task<bool> ValidateSectionIdsAsync(StartExamPracticeCommand request, MethodResult<ExamPracticeResultModel> methodResult)
        {
            if (request.ExamPracticeSectionIds != null && request.ExamPracticeSectionIds.Any())
            {
                var examPracticeSections = await _examPracticeSectionRepository.GetByIdsAsync(request.ExamPracticeSectionIds);
                if (!examPracticeSections.Any())
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPracticeSections), request.ExamPracticeSectionIds);
                    return false;
                }
                if (examPracticeSections.Count() != request.ExamPracticeSectionIds.Count)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPracticeSections), request.ExamPracticeSectionIds.Where(x => examPracticeSections.Any(y => y.Id == x)));
                    return false;
                }
            }
            return true;
        }

        private static bool ValidateSkillMockTest(StartExamPracticeCommand request, ExamPractice examPractice, MethodResult<ExamPracticeResultModel> methodResult)
        {
            var examPracticeSection = examPractice.ExamPracticeSections.FirstOrDefault();
            if (examPracticeSection != null && examPracticeSection.CourseSkill.HasValue && examPractice.SubType == EnumExamPracticeSubType.SkillMockTest && request.PracticeMode == EnumPracticeMode.Practice)
            {
                var listSkill = new List<EnumCourseSkill> { EnumCourseSkill.Reading, EnumCourseSkill.Writing };
                var isPartExamPractice = listSkill.Any(x => x == examPracticeSection.CourseSkill.Value);

                if (isPartExamPractice && (request.ExamPracticeSectionIds == null || !request.ExamPracticeSectionIds.Any()))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(EnumExamPracticeSubType.SkillMockTest), request.ExamPracticeSectionIds);
                    return false;
                }
                if (isPartExamPractice && !request.PracticeTimeLimitOption.HasValue)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(EnumExamPracticeSubType.SkillMockTest), request.PracticeTimeLimitOption);
                    return false;
                }
                if (!isPartExamPractice && request.ExamPracticeSectionIds != null && request.ExamPracticeSectionIds.Any())
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(EnumExamPracticeSubType.SkillMockTest), request.ExamPracticeSectionIds);
                    return false;
                }
            }
            return true;
        }

        private static bool ValidateExamPracticeType(StartExamPracticeCommand request, ExamPractice examPractice, MethodResult<ExamPracticeResultModel> methodResult)
        {
            if (examPractice.Type == EnumExamPracticeType.ExamPractice && request.PracticeMode == EnumPracticeMode.Practice)
            {
                if (request.ExamPracticeSectionIds != null && request.ExamPracticeSectionIds.Any())
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(EnumExamPracticeType.ExamPractice), request.ExamPracticeSectionIds);
                    return false;
                }
                if (!request.PracticeTimeLimitOption.HasValue)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(EnumExamPracticeType.ExamPractice), request.PracticeTimeLimitOption);
                    return false;
                }
            }
            return true;
        }

        private async Task<ExamPracticeRetry?> GetOrCreateExamPracticeRetryAsync(Guid studentId, Guid examPracticeId, MethodResult<ExamPracticeResultModel> methodResult, CancellationToken cancellationToken)
        {
            var examPracticeRetry = await _examPracticeRetryRepository.Queryable.AsQueryable().FirstOrDefaultAsync(x => x.ExamPracticeId == examPracticeId && x.StudentId == studentId, cancellationToken);
            if (examPracticeRetry == null)
            {
                examPracticeRetry = new ExamPracticeRetry
                {
                    RetryCount = MaxRetryAttempts,
                    StudentId = studentId,
                    ExamPracticeId = examPracticeId,
                };
                await _examPracticeRetryRepository.ExecuteTransactionAsync(async () =>
                {
                    await _examPracticeRetryRepository.BulkMergeAsync(new List<ExamPracticeRetry> { examPracticeRetry }, bulk =>
                    {
                        bulk.ColumnPrimaryKeyExpression = entity => new { entity.StudentId, entity.ExamPracticeId };
                    });
                    return methodResult;
                });
            }
            return examPracticeRetry;
        }

        private static bool ValidateExamPracticeResult(ExamPracticeResult? examPracticeResult, MethodResult<ExamPracticeResultModel> methodResult)
        {
            if (examPracticeResult != null && examPracticeResult.Status == EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumExamPracticeErrorCode.TestJustSubmittedOnAnotherDevice), nameof(examPracticeResult));
                return false;
            }
            if (examPracticeResult != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumExamPracticeErrorCode.SessionOnOtherDevice), nameof(examPracticeResult));
                return false;
            }
            return true;
        }

        private async Task<ExamPracticeResult> CreateExamPracticeResultAsync(
            StartExamPracticeCommand request,
            Guid studentId,
            ExamPractice examPractice,
            ExamPracticeRetry examPracticeRetry,
            CancellationToken cancellationToken)
        {
            var resultPosition = await _examPracticeResultRepository.Queryable
                .Where(x => x.ExamPracticeId == examPracticeRetry.Id)
                .CountAsync(cancellationToken);

            var examPracticeResult = new ExamPracticeResult
            {
                StudentId = studentId,
                ExamPracticeId = examPractice.Id,
                ExamPracticeRetryId = examPracticeRetry.Id,
                Status = EnumResultStatus.Process,
                WorkingStatus = EnumWorkingStatus.Active,
                PracticeMode = request.PracticeMode,
                ResultPosition = resultPosition,
            };

            examPracticeResult.Config = await BuildExerciseConfigAsync(request, examPractice, cancellationToken);
            return examPracticeResult;
        }

        private async Task<ExerciseConfig?> BuildExerciseConfigAsync(StartExamPracticeCommand request, ExamPractice examPractice, CancellationToken cancellationToken)
        {
            if (request.PracticeMode != EnumPracticeMode.Practice)
            {
                return null;
            }
            var exerciseConfig = new ExerciseConfig
            {
                ExamPracticeSectionIds = request.ExamPracticeSectionIds,
                IsAllPart = request.IsAllPart,
                PracticeTimeLimitOption = request.PracticeTimeLimitOption,
            };

            if (request.PracticeTimeLimitOption.HasValue)
            {
                if (request.PracticeTimeLimitOption != EnumPracticeTimeLimitOption.ExamBased)
                {
                    exerciseConfig.ExecutionTime = (int)request.PracticeTimeLimitOption.Value;
                }
                else if (examPractice.Type == EnumExamPracticeType.IELTS)
                {
                    var examPracticeSections = await _examPracticeSectionRepository.Queryable
                        .Where(x => x.ExamPracticeId == examPractice.Id)
                        .ToListAsync(cancellationToken);
                    exerciseConfig.ExecutionTime = (int?)examPracticeSections.Sum(x => x.Config?.ExecutionTime ?? default);
                }
                else
                {
                    exerciseConfig.ExecutionTime = examPractice.ExecutionTime;
                }
            }
            return exerciseConfig;
        }
    }
}
