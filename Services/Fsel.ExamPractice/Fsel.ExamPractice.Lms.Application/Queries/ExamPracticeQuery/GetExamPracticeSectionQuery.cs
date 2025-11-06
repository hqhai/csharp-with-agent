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
            var examPracticeResult = await GetExamPracticeResultAsync(request.ExamPracticeResultId, methodResult);
            if (examPracticeResult == null)
            {
                return methodResult;
            }

            var examPracticeSection = await GetExamPracticeSectionAsync(request.ExamPracticeSectionId, request.ExamPracticeResultId, methodResult);
            if (examPracticeSection == null)
            {
                return methodResult;
            }

            var examPracticeSectionResult = await EnsureSectionResultAsync(examPracticeResult, examPracticeSection, methodResult);
            var examPracticeSections = await GetChildSectionsAsync(examPracticeResult, examPracticeSection.Id, cancellationToken);
            var examPracticeAnswers = await GetAnswersAsync(request.ExamPracticeResultId, cancellationToken);

            var examPracticeSectionDetail = _mapper.Map<ExamPracticeSectionDetailModel>(examPracticeSection);
            var examPracticeSectionResultModel = _mapper.Map<ExamPracticeSectionResultModel>(examPracticeSectionResult);
            examPracticeSectionResultModel.RemainingTime = CalculateRemainingTime(examPracticeResult, examPracticeSectionResult, examPracticeSection);

            var examPracticeSectionDetails = MapChildSectionDetails(examPracticeSections, examPracticeAnswers, isResultDone: examPracticeResult.Status == EnumResultStatus.Done);
            var executionTime = CalculateExecutionTime(examPracticeResult, examPracticeSection);

            // 5) Compute execution time for the UI
            examPracticeSectionDetail.ExamPracticeSectionResult = examPracticeSectionResultModel;
            examPracticeSectionDetail.ExamPracticeSections = examPracticeSectionDetails;
            examPracticeSectionDetail.ExecutionTime = executionTime;
            methodResult.Result = examPracticeSectionDetail;
            return methodResult;
        }

        private List<ExamPracticeSectionDetailModel> MapChildSectionDetails(IEnumerable<ExamPracticeSection> examPracticeSections, IEnumerable<ExamPracticeAnswer> examPracticeAnswers, bool isResultDone)
        {
            var answerByQuestionId = examPracticeAnswers
                .Where(a => a.QuestionId.HasValue)
                .GroupBy(a => a.QuestionId!.Value)
                .ToDictionary(g => g.Key, g => g.First()); // nếu trùng lấy cái đầu

            var examPracticeSectionDetails = new List<ExamPracticeSectionDetailModel>();

            foreach (var examPracticeSection in examPracticeSections)
            {
                var examPracticeSectionDto = _mapper.Map<ExamPracticeSectionDetailModel>(examPracticeSection);
                examPracticeSectionDto.QuestionTests = BuildQuestionStatuses(examPracticeSection.Questions, answerByQuestionId, isResultDone);
                examPracticeSectionDetails.Add(examPracticeSectionDto);
            }

            return examPracticeSectionDetails;
        }

        private static List<QuestionCorrectStatusModel> BuildQuestionStatuses(IEnumerable<Question> questions, IReadOnlyDictionary<Guid, ExamPracticeAnswer> answersByQuestion, bool isResultDone)
        {
            return questions
                .OrderBy(q => q.CreatedDate)
                .Select(q =>
                {
                    answersByQuestion.TryGetValue(q.Id, out var answer);
                    return new QuestionCorrectStatusModel
                    {
                        QuestionId = q.Id,
                        Status = EnumHelper.GetStatus(answer, isResultDone)
                    };
                })
                .ToList();
        }

        private static double CalculateRemainingTime(ExamPracticeResult examPracticeResult, ExamPracticeSectionResult sectionResult, ExamPracticeSection examPracticeSection)
        {
            double totalExecutionTime;
            if (examPracticeResult.PracticeMode == EnumPracticeMode.Practice && examPracticeResult.Config?.PracticeTimeLimitOption != EnumPracticeTimeLimitOption.ExamBased)
            {
                totalExecutionTime = examPracticeResult.Config?.ExecutionTime ?? default;
            }
            else
            {
                totalExecutionTime = examPracticeSection.Config?.ExecutionTime ?? default;
            }
            var remain = totalExecutionTime - sectionResult.WorkingTime;
            return remain > 0 ? remain : default;
        }

        private static double CalculateExecutionTime(ExamPracticeResult examPracticeResult, ExamPracticeSection section)
        {
            if (examPracticeResult.PracticeMode == EnumPracticeMode.Practice && examPracticeResult.Config?.PracticeTimeLimitOption != EnumPracticeTimeLimitOption.ExamBased)
            {
                return examPracticeResult.Config?.ExecutionTime ?? default;
            }

            return section.Config?.ExecutionTime ?? default;
        }

        private async Task<List<ExamPracticeSection>> GetChildSectionsAsync(ExamPracticeResult examPracticeResult, Guid parentSectionId, CancellationToken ct)
        {
            var baseQuery = _examPracticeSectionRepository.Queryable
                                .Include(x => x.Questions)
                                .Include(x => x.ExamPracticeAnswers.Where(x => x.ExamPracticeResultId == examPracticeResult.Id))
                                .Include(x => x.ExamPracticeSections)
                                .ThenInclude(x => x.ExamPracticeAnswers.Where(x => x.ExamPracticeResultId == examPracticeResult.Id))
                                .Where(x => x.ParentExamPracticeSectionId == parentSectionId)
                                .AsNoTracking();

            // If config null or IsAllPart = true -> lấy tất cả phần con
            if (examPracticeResult.Config == null || examPracticeResult.Config.IsAllPart)
            {
                return await baseQuery.OrderBy(x => x.CreatedDate).ToListAsync(ct);
            }

            // Nếu có danh sách phần cụ thể
            if (examPracticeResult.Config.ExamPracticeSectionIds is { Count: > 0 })
            {
                baseQuery = baseQuery.WhereBulkContains(examPracticeResult.Config.ExamPracticeSectionIds, x => x.Id);
            }

            // Config có nhưng rỗng => không có phần nào
            return await baseQuery.OrderBy(x => x.CreatedDate).ToListAsync(ct);
        }

        private async Task<List<ExamPracticeAnswer>> GetAnswersAsync(Guid examPracticeResultId, CancellationToken ct)
        {
            return await _examPracticeAnswerRepository.Queryable
                .Where(x => x.ExamPracticeResultId == examPracticeResultId && x.QuestionId.HasValue)
                .AsNoTracking()
                .ToListAsync(ct);
        }

        private async Task<ExamPracticeSectionResult> EnsureSectionResultAsync(ExamPracticeResult examPracticeResult, ExamPracticeSection examPracticeSection, MethodResult<ExamPracticeSectionDetailModel> methodResult)
        {
            // Try read existing (no tracking is fine here)
            var examPracticeSectionResult = await _examPracticeSectionResultRepository.Queryable
                                                  .Where(x => x.ExamPracticeSectionId == examPracticeSection.Id)
                                                  .Where(x => x.ExamPracticeResultId == examPracticeResult.Id)
                                                  .AsNoTracking()
                                                  .FirstOrDefaultAsync();

            if (examPracticeSectionResult == null)
            {
                // Insert idempotently via BulkMerge on natural key
                examPracticeSectionResult = new ExamPracticeSectionResult
                {
                    ExamPracticeSectionId = examPracticeSection.Id,
                    StudentId = examPracticeResult.StudentId,
                    ExamPracticeResultId = examPracticeResult.Id,
                    Status = EnumResultStatus.New
                };
                try
                {
                    await _examPracticeSectionResultRepository.ExecuteTransactionAsync(async () =>
                    {
                        await _examPracticeSectionResultRepository.BulkMergeAsync(new List<ExamPracticeSectionResult> { examPracticeSectionResult },
                            bulk => bulk.ColumnPrimaryKeyExpression = e => new { e.StudentId, e.ExamPracticeSectionId, e.ExamPracticeResultId }
                        );
                        return methodResult;
                    });
                }
                catch
                {
                }
            }

            return examPracticeSectionResult;
        }

        private async Task<ExamPracticeResult?> GetExamPracticeResultAsync(Guid id, MethodResult<ExamPracticeSectionDetailModel> result)
        {
            var examPracticeResult = await _examPracticeResultRepository.GetByIdAsync(id);
            if (examPracticeResult == null)
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(ExamPracticeResult), id);
            }
            return examPracticeResult;
        }

        private async Task<ExamPracticeSection?> GetExamPracticeSectionAsync(Guid id, Guid examPracticeResultId, MethodResult<ExamPracticeSectionDetailModel> result)
        {
            var examPracticeSection = await _examPracticeSectionRepository.Queryable.AsNoTracking().Include(x => x.ExamPracticeScores.Where(x => x.ExamPracticeResultId == examPracticeResultId))
                                                                          .FirstOrDefaultAsync(x => x.Id == id);
            if (examPracticeSection == null)
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(ExamPracticeSection), id);
            }
            return examPracticeSection;
        }
    }
}
