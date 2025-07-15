// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Queries.ExamPracticeQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
    using Fsel.ExamPractice.Domain.Models.EntityModels.Questions;
    using Fsel.ExamPractice.Infrastructure.Common;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetExamPracticeSectionQuery : IRequest<MethodResult<ExamPracticeSectionDetailModel>>
    {
        public Guid ExamPracticeResultId { get; set; }
        public Guid ExamPracticeSectionId { get; set; }
    }

    public class GetExamPracticeSectionQueryHandler : IRequestHandler<GetExamPracticeSectionQuery, MethodResult<ExamPracticeSectionDetailModel>>
    {
        private readonly IExamPracticeSectionRepository _examPracticeSectionRepository;
        private readonly IExamPracticeResultRepository _examPracticeResultRepository;
        private readonly IMapper _mapper;
        private readonly IExamPracticeSectionResultRepository _examPracticeSectionResultRepository;
        private readonly IExamPracticeAnswerRepository _examPracticeAnswerRepository;

        public GetExamPracticeSectionQueryHandler(
            IExamPracticeSectionRepository examPracticeSectionRepository,
            IExamPracticeResultRepository examPracticeResultRepository,
            IMapper mapper,
            IExamPracticeSectionResultRepository examPracticeSectionResultRepository,
            IExamPracticeAnswerRepository examPracticeAnswerRepository)
        {
            _examPracticeSectionRepository = examPracticeSectionRepository;
            _examPracticeResultRepository = examPracticeResultRepository;
            _mapper = mapper;
            _examPracticeSectionResultRepository = examPracticeSectionResultRepository;
            _examPracticeAnswerRepository = examPracticeAnswerRepository;
        }

        public async Task<MethodResult<ExamPracticeSectionDetailModel>> Handle(GetExamPracticeSectionQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ExamPracticeSectionDetailModel>();

            var examPracticeResult = await _examPracticeResultRepository.GetByIdAsync(request.ExamPracticeResultId);
            if (examPracticeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPracticeResult));
                return methodResult;
            }

            var examPracticeSection = await _examPracticeSectionRepository.Queryable.Where(x => x.ExamPracticeId == examPracticeResult.ExamPracticeId)
                                            .Where(x => x.Id == request.ExamPracticeSectionId).FirstOrDefaultAsync(cancellationToken);
            if (examPracticeSection == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPracticeSection), request.ExamPracticeSectionId);
                return methodResult;
            }

            var examPracticeSectionResult = await _examPracticeSectionResultRepository.Queryable.Where(x => x.ExamPracticeSectionId == request.ExamPracticeSectionId)
                                                                    .FirstOrDefaultAsync(x => x.ExamPracticeResultId == request.ExamPracticeResultId, cancellationToken);
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
            var examPracticeSections = new List<ExamPracticeSection>();
            if (examPracticeResult.Config == null || (examPracticeResult.Config != null && examPracticeResult.Config.IsAllPart))
            {
                examPracticeSections = await _examPracticeSectionRepository.Queryable
                                                                      .Include(x => x.Questions)
                                                                      .Where(x => x.ParentExamPracticeSectionId == examPracticeSection.Id)
                                                                      .ToListAsync(cancellationToken);
            }
            else if (examPracticeResult.Config != null && examPracticeResult.Config.ExamPracticeSectionIds != null && examPracticeResult.Config.ExamPracticeSectionIds.Any())
            {
                examPracticeSections = await _examPracticeSectionRepository.Queryable
                                                                .Include(x => x.Questions)
                                                                .Where(x => x.ParentExamPracticeSectionId == examPracticeSection.Id)
                                                                .WhereBulkContains(examPracticeResult.Config.ExamPracticeSectionIds, x => x.Id)
                                                                .ToListAsync(cancellationToken);
            }

            var examPracticeSectionDetail = _mapper.Map<ExamPracticeSectionDetailModel>(examPracticeSection);
            var examPracticeSectionDetails = new List<ExamPracticeSectionDetailModel>();
            var examPracticeAnswers = await _examPracticeAnswerRepository.Queryable.Where(x => x.ExamPracticeResultId == examPracticeResult.Id && x.QuestionId.HasValue).ToListAsync(cancellationToken);

            foreach (var item in examPracticeSections)
            {
                var examPracticeSectionDto = _mapper.Map<ExamPracticeSectionDetailModel>(item);
                examPracticeSectionDto.QuestionTests = item.Questions.OrderBy(x => x.CreatedDate)
                                            .Select(x =>
                                            {
                                                var answer = examPracticeAnswers.FirstOrDefault(y => y.QuestionId == x.Id);
                                                return new QuestionCorrectStatusModel
                                                {
                                                    QuestionId = x.Id,
                                                    Status = EnumHelper.GetStatus(answer, examPracticeResult.Status == EnumResultStatus.Done)
                                                };
                                            }).ToList();
                examPracticeSectionDetails.Add(examPracticeSectionDto);
            }
            var examPracticeSectionResultModel = _mapper.Map<ExamPracticeSectionResultModel>(examPracticeSectionResult);
            double executionTime = default;
            if (examPracticeResult.PracticeMode == EnumPracticeMode.Practice && examPracticeResult.Config?.PracticeTimeLimitOption != EnumPracticeTimeLimitOption.ExamBased)
            {
                executionTime = examPracticeResult.Config?.ExecutionTime ?? default;
                examPracticeSectionResultModel.RemainingTime = executionTime - examPracticeSectionResult.WorkingTime > 0 ? executionTime - examPracticeSectionResult.WorkingTime : default;
            }
            else
            {
                executionTime = examPracticeSection.Config?.ExecutionTime ?? default;
            }
            examPracticeSectionDetail.ExamPracticeSectionResult = examPracticeSectionResultModel;
            examPracticeSectionDetail.ExamPracticeSections = examPracticeSectionDetails;
            examPracticeSectionDetail.ExecutionTime = executionTime;
            methodResult.Result = examPracticeSectionDetail;
            return methodResult;
        }
    }
}
