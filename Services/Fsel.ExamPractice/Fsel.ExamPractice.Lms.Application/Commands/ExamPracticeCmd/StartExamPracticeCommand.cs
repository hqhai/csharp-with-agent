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
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
    using Fsel.ExamPractice.Lms.Application.Services.UserServices;
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
        private readonly IExamPracticeRepository _examPracticeRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IExamPracticeRetryRepository _examPracticeRetryRepository;
        private readonly IExamPracticeResultRepository _examPracticeResultRepository;
        private readonly IMapper _mapper;
        private const int MaxRetryAttempts = 20;

        public StartExamPracticeCommandHandler(
            IExamPracticeRepository examPracticeRepository,
            AuthContext authContext,
            IUserService userService,
            IExamPracticeRetryRepository examPracticeRetryRepository,
            IExamPracticeResultRepository examPracticeResultRepository,
            IMapper mapper)
        {
            _examPracticeRepository = examPracticeRepository;
            _authContext = authContext;
            _userService = userService;
            _examPracticeRetryRepository = examPracticeRetryRepository;
            _examPracticeResultRepository = examPracticeResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<ExamPracticeResultModel>> Handle(StartExamPracticeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ExamPracticeResultModel>();
            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var examPractice = await _examPracticeRepository.Queryable.Include(x => x.ExamPracticeSections).FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (examPractice == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPractice), request.Id);
                return methodResult;
            }
            if (examPractice.SubType == EnumExamPracticeSubType.FullMockTest)
            {
                if (request.PracticeMode == EnumPracticeMode.Practice)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(EnumExamPracticeSubType.FullMockTest), request.PracticeMode);
                    return methodResult;
                }
                if (request.ExamPracticeSectionIds != null && request.ExamPracticeSectionIds.Any())
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(EnumExamPracticeSubType.FullMockTest), request.ExamPracticeSectionIds);
                    return methodResult;
                }
                if (request.PracticeTimeLimitOption.HasValue)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(EnumExamPracticeSubType.FullMockTest), request.PracticeTimeLimitOption);
                    return methodResult;
                }
            }
            var examPracticeSection = examPractice.ExamPracticeSections.FirstOrDefault();
            if (examPracticeSection != null && examPracticeSection.CourseSkill.HasValue && examPractice.SubType == EnumExamPracticeSubType.SkillMockTest && request.PracticeMode == EnumPracticeMode.Practice)
            {
                var listSkill = new List<EnumCourseSkill> { EnumCourseSkill.Reading, EnumCourseSkill.Writing };
                var isPartExamPractice = listSkill.Any(x => x == examPracticeSection.CourseSkill.Value);

                if (isPartExamPractice && (request.ExamPracticeSectionIds == null || !request.ExamPracticeSectionIds.Any()))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(EnumExamPracticeSubType.SkillMockTest), request.ExamPracticeSectionIds);
                    return methodResult;
                }
                if (isPartExamPractice && !request.PracticeTimeLimitOption.HasValue)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(EnumExamPracticeSubType.SkillMockTest), request.PracticeTimeLimitOption);
                    return methodResult;
                }
                if (!isPartExamPractice && request.ExamPracticeSectionIds != null && request.ExamPracticeSectionIds.Any())
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(EnumExamPracticeSubType.SkillMockTest), request.ExamPracticeSectionIds);
                    return methodResult;
                }
            }
            if (examPractice.Type == EnumExamPracticeType.ExamPractice && request.PracticeMode == EnumPracticeMode.Practice)
            {
                if (request.ExamPracticeSectionIds != null && request.ExamPracticeSectionIds.Any())
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(EnumExamPracticeType.ExamPractice), request.ExamPracticeSectionIds);
                    return methodResult;
                }
                if (!request.PracticeTimeLimitOption.HasValue)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(EnumExamPracticeType.ExamPractice), request.PracticeTimeLimitOption);
                    return methodResult;
                }
            }

            var examPracticeRetry = await _examPracticeRetryRepository.Queryable.FirstOrDefaultAsync(x => x.ExamPracticeId == examPractice.Id && x.StudentId == student.Id, cancellationToken);
            if (examPracticeRetry == null)
            {
                examPracticeRetry = new ExamPracticeRetry
                {
                    RetryCount = MaxRetryAttempts,
                    StudentId = student.Id,
                    ExamPracticeId = examPractice.Id,
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
            var examPracticeResult = await _examPracticeResultRepository.Queryable
                                          .Where(x => x.ExamPracticeRetryId == examPracticeRetry.Id && x.WorkingStatus == EnumWorkingStatus.Active)
                                          .FirstOrDefaultAsync(cancellationToken);
            if (examPracticeResult != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(examPracticeResult));
                return methodResult;
            }

            examPracticeResult = new ExamPracticeResult
            {
                StudentId = student.Id,
                ExamPracticeId = examPractice.Id,
                ExamPracticeRetryId = examPracticeRetry.Id,
                Status = EnumResultStatus.Process,
                WorkingStatus = EnumWorkingStatus.Active,
                PracticeMode = request.PracticeMode,
                ResultPosition = await _examPracticeResultRepository.Queryable.Where(x => x.ExamPracticeId == examPracticeRetry.Id).CountAsync(cancellationToken),
                Config = request.PracticeMode == EnumPracticeMode.Practice ? new ExerciseConfig
                {
                    ExamPracticeSectionIds = request.ExamPracticeSectionIds,
                    IsAllPart = request.IsAllPart,
                    PracticeTimeLimitOption = request.PracticeTimeLimitOption,
                    ExecutionTime = request.PracticeTimeLimitOption.HasValue && request.PracticeTimeLimitOption != EnumPracticeTimeLimitOption.ExamBased ? (int)request.PracticeTimeLimitOption.Value : null,
                } : null
            };
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
    }
}
