// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Commands.ExamPracticeCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Models.CommandModels.ExamPracticeAnswers;
    using Fsel.ExamPractice.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class CreateExamPracticeAnswerCommand : CreateExamPracticeAnswerCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateExamPracticeAnswerCommandHandler : IRequestHandler<CreateExamPracticeAnswerCommand, MethodResult<bool>>
    {
        private readonly IExamPracticeRepository _examPracticeRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IExamPracticeResultRepository _examPracticeResultRepository;
        private readonly IExamPracticeSectionRepository _examPracticeSectionRepository;
        private readonly IExamPracticeSectionResultRepository _examPracticeSectionResultRepository;

        public CreateExamPracticeAnswerCommandHandler(
            IExamPracticeRepository examPracticeRepository,
            AuthContext authContext,
            IUserService userService,
            IExamPracticeResultRepository examPracticeResultRepository,
            IExamPracticeSectionRepository examPracticeSectionRepository,
            IExamPracticeSectionResultRepository examPracticeSectionResultRepository)
        {
            _examPracticeRepository = examPracticeRepository;
            _authContext = authContext;
            _userService = userService;
            _examPracticeResultRepository = examPracticeResultRepository;
            _examPracticeSectionRepository = examPracticeSectionRepository;
            _examPracticeSectionResultRepository = examPracticeSectionResultRepository;
        }

        public async Task<MethodResult<bool>> Handle(CreateExamPracticeAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
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
            var examPracticeResult = await _examPracticeResultRepository.GetByIdAsync(request.ExamPracticeResultId);
            if (examPracticeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPracticeResult), request.ExamPracticeResultId);
                return methodResult;
            }
            if (examPracticeResult.Status == EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusDone), nameof(examPracticeResult.Status), examPracticeResult.Status);
                return methodResult;
            }
            var examPractice = await _examPracticeRepository.GetByIdAsync(examPracticeResult.ExamPracticeId);
            if (examPractice == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPractice), examPracticeResult.ExamPracticeId);
                return methodResult;
            }

            var examPracticeSection = await _examPracticeSectionRepository.GetByIdAsync(request.ExamPracticeSectionId);
            if (examPracticeSection == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPracticeSection), request.ExamPracticeSectionId);
                return methodResult;
            }
            if (examPractice.Type == EnumExamPracticeType.IELTS)
            {
                var examPracticeSectionResult = await _examPracticeSectionResultRepository.Queryable.Where(x => x.StudentId == student.Id && x.ExamPracticeSectionId == examPracticeSection.Id && x.ExamPracticeResultId == examPracticeResult.Id)
                                                                                   .FirstOrDefaultAsync(cancellationToken);
                if (examPracticeSectionResult == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPracticeSectionResult));
                    return methodResult;
                }
                if (examPracticeSection.CourseSkill.HasValue && request.Answers != null && request.Answers.Any())
                {
                    var listSkill = new List<EnumCourseSkill>() { EnumCourseSkill.Reading, EnumCourseSkill.Listening };
                    if (listSkill.Contains(examPracticeSection.CourseSkill.Value))
                    {
                        examPracticeSectionResult = new ExamPracticeSectionResult
                        {
                            ExamPracticeSectionId = examPracticeSection.Id,
                            StudentId = examPracticeResult.StudentId,
                            ExamPracticeResultId = examPracticeResult.Id,
                            Status = EnumResultStatus.New,
                        };
                        await _examPracticeSectionResultRepository.ExecuteTransactionAsync(async () =>
                        {
                            await _examPracticeSectionResultRepository.BulkMergeAsync(new List<ExamPracticeSectionResult> { examPracticeSectionResult }, bulk =>
                            {
                                bulk.ColumnPrimaryKeyExpression = entity => new { entity.StudentId, entity.ExamPracticeSectionId, entity.ExamPracticeResultId };
                            });
                            return methodResult;
                        });
                    }
                    else if (examPracticeSection.CourseSkill.Value == EnumCourseSkill.Speaking)
                    {
                        examPracticeSectionResult = new ExamPracticeSectionResult
                        {
                            ExamPracticeSectionId = examPracticeSection.Id,
                            StudentId = examPracticeResult.StudentId,
                            ExamPracticeResultId = examPracticeResult.Id,
                            Status = EnumResultStatus.New,
                        };
                        await _examPracticeSectionResultRepository.ExecuteTransactionAsync(async () =>
                        {
                            await _examPracticeSectionResultRepository.BulkMergeAsync(new List<ExamPracticeSectionResult> { examPracticeSectionResult }, bulk =>
                            {
                                bulk.ColumnPrimaryKeyExpression = entity => new { entity.StudentId, entity.ExamPracticeSectionId, entity.ExamPracticeResultId };
                            });
                            return methodResult;
                        });
                    }
                }
            }
            else
            {
                var examPracticeSectionResult = await _examPracticeSectionResultRepository.Queryable.Where(x => x.StudentId == student.Id && x.ExamPracticeSectionId == examPracticeSection.Id && x.ExamPracticeResultId == examPracticeResult.Id)
                                                                                   .FirstOrDefaultAsync(cancellationToken);
                if (examPracticeSectionResult == null)
                {
                    examPracticeSectionResult = new ExamPracticeSectionResult
                    {
                        ExamPracticeSectionId = examPracticeSection.Id,
                        StudentId = examPracticeResult.StudentId,
                        ExamPracticeResultId = examPracticeResult.Id,
                        Status = EnumResultStatus.New,
                    };
                    await _examPracticeSectionResultRepository.ExecuteTransactionAsync(async () =>
                    {
                        await _examPracticeSectionResultRepository.BulkMergeAsync(new List<ExamPracticeSectionResult> { examPracticeSectionResult }, bulk =>
                        {
                            bulk.ColumnPrimaryKeyExpression = entity => new { entity.StudentId, entity.ExamPracticeSectionId, entity.ExamPracticeResultId };
                        });
                        return methodResult;
                    });
                }
            }

            return methodResult;
        }
    }
}
