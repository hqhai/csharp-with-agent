namespace Fsel.ExamPractice.Lms.Application.Commands.ExamPracticeCmd
{
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Models.CommandModels.ExamPractices;
    using Fsel.ExamPractice.Infrastructure.Common;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SetTimeExamPracticeCommand : SetTimeExamPracticeCommandModel, IRequest<bool>
    {
    }

    public class SetTimeExamPracticeCommandHandler : IRequestHandler<SetTimeExamPracticeCommand, bool>
    {
        private readonly IExamPracticeSectionRepository _examPracticeSectionRepository;
        private readonly IExamPracticeSectionResultRepository _examPracticeSectionResultRepository;
        private readonly IExamPracticeResultRepository _examPracticeResultRepository;
        private readonly IExamPracticeRepository _examPractice;

        public SetTimeExamPracticeCommandHandler(
            IExamPracticeSectionRepository examPracticeSectionRepository,
            IExamPracticeSectionResultRepository examPracticeSectionResultRepository,
            IExamPracticeResultRepository examPracticeResultRepository,
            IExamPracticeRepository examPractice)
        {
            _examPracticeSectionRepository = examPracticeSectionRepository;
            _examPracticeSectionResultRepository = examPracticeSectionResultRepository;
            _examPracticeResultRepository = examPracticeResultRepository;
            _examPractice = examPractice;
        }

        public async Task<bool> Handle(SetTimeExamPracticeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.Type != EnumExamPracticeType.ExamPractice.ToString())
            {
                return await HandleType(request, cancellationToken);
            }
            else
            {
                return await HandleOtherType(request, cancellationToken);
            }
        }

        private async Task<bool> HandleType(SetTimeExamPracticeCommand request, CancellationToken cancellationToken)
        {
            var examPracticeSectionResult = await (from a in _examPracticeSectionResultRepository.Queryable
                                                   join b in _examPracticeSectionRepository.Queryable on a.ExamPracticeSectionId equals b.Id
                                                   where a.Id == request.ObjectId
                                                   select new
                                                   {
                                                       ExamPracticeSectionResult = a,
                                                       ExamPracticeSection = b
                                                   }).FirstOrDefaultAsync(cancellationToken);

            if (examPracticeSectionResult == null)
            {
                return false;
            }
            var examPracticeResult = await (from a in _examPracticeResultRepository.Queryable
                                            join b in _examPractice.Queryable on a.ExamPracticeId equals b.Id
                                            where a.Id == examPracticeSectionResult.ExamPracticeSectionResult.ExamPracticeResultId
                                            select new
                                            {
                                                ExamPracticeResult = a,
                                                ExamPractice = b
                                            }).FirstOrDefaultAsync(cancellationToken);

            if (examPracticeResult == null)
            {
                return false;
            }

            double executionTime = GetExecutionTimeForIelts(examPracticeResult, examPracticeSectionResult);
            examPracticeSectionResult.ExamPracticeSectionResult.WorkingTime = DateTimeHelper.SetWorkingTime(
                    examPracticeSectionResult.ExamPracticeSectionResult.WorkingTime,
                    request.AccessTime,
                    executionTime);

            await _examPracticeSectionResultRepository.BulkUpdateList(new List<ExamPracticeSectionResult> { examPracticeSectionResult.ExamPracticeSectionResult },
            bulk =>
            {
                bulk.ColumnInputExpression = entity => new { entity.WorkingTime };
            });

            return true;
        }

        private async Task<bool> HandleOtherType(SetTimeExamPracticeCommand request, CancellationToken cancellationToken)
        {
            var examPracticeResult = await (from a in _examPracticeResultRepository.Queryable
                                            join b in _examPractice.Queryable on a.ExamPracticeId equals b.Id
                                            where a.Id == request.ObjectId
                                            select new
                                            {
                                                ExamPracticeResult = a,
                                                ExamPractice = b
                                            }).FirstOrDefaultAsync(cancellationToken);

            if (examPracticeResult == null)
            {
                return false;
            }

            double executionTime = GetExecutionTimeForOther(examPracticeResult);

            examPracticeResult.ExamPracticeResult.WorkingTime = DateTimeHelper.SetWorkingTime(examPracticeResult.ExamPracticeResult.WorkingTime, request.AccessTime, executionTime);

            await _examPracticeResultRepository.BulkUpdateList(new List<ExamPracticeResult> { examPracticeResult.ExamPracticeResult },
                bulk =>
                {
                    bulk.ColumnInputExpression = entity => new { entity.WorkingTime };
                }
            );

            return true;
        }

        private static double GetExecutionTimeForIelts(dynamic examPracticeResult, dynamic examPracticeSectionResult)
        {
            if (examPracticeResult.ExamPractice.SubType == EnumExamPracticeSubType.SkillMockTest &&
                examPracticeResult.ExamPracticeResult.PracticeMode != null &&
                examPracticeResult.ExamPracticeResult.PracticeMode == EnumPracticeMode.Practice)
            {
                return examPracticeResult.ExamPracticeResult.Config?.ExecutionTime ?? default;
            }
            else
            {
                return examPracticeSectionResult.ExamPracticeSection?.Config?.ExecutionTime ?? default;
            }
        }

        private static double GetExecutionTimeForOther(dynamic examPracticeResult)
        {
            if (examPracticeResult.ExamPracticeResult.PracticeMode != null && examPracticeResult.ExamPracticeResult.PracticeMode == EnumPracticeMode.Practice)
            {
                return examPracticeResult.ExamPracticeResult.Config?.ExecutionTime ?? default;
            }
            else
            {
                return examPracticeResult.ExamPractice?.ExecutionTime ?? default;
            }
        }
    }
}
