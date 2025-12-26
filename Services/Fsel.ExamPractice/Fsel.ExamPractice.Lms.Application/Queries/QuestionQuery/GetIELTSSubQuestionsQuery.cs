// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Queries.QuestionQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Entities.QuestionTypeConfigs.Answers.V1i1;
    using Fsel.ExamPractice.Domain.Entities.QuestionTypeConfigs.Questions.V1i1;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Models.EntityModels.Questions;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetIELTSSubQuestionsQuery : IRequest<MethodResult<IList<SubQuestionModel>>>
    {
        public Guid ObjectResultId { get; set; }
        public Guid ExamPracticeSectionId { get; set; }
    }

    public class GetIELTSSubQuestionsQueryHandler : IRequestHandler<GetIELTSSubQuestionsQuery, MethodResult<IList<SubQuestionModel>>>
    {
        private readonly IExamPracticeSectionRepository _examPracticeSectionRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IExamPracticeSectionResultRepository _examPracticeSectionResultRepository;

        public GetIELTSSubQuestionsQueryHandler(IExamPracticeSectionRepository examPracticeSectionRepository, IQuestionRepository questionRepository, IExamPracticeSectionResultRepository examPracticeSectionResultRepository)
        {
            _examPracticeSectionRepository = examPracticeSectionRepository;
            _questionRepository = questionRepository;
            _examPracticeSectionResultRepository = examPracticeSectionResultRepository;
        }

        public async Task<MethodResult<IList<SubQuestionModel>>> Handle(GetIELTSSubQuestionsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<SubQuestionModel>>();

            var examPracticeSectionResult = await _examPracticeSectionResultRepository.Queryable.AsNoTracking()
                                                                                      .Include(x => x.ExamPracticeAnswers)
                                                                                      .Where(x => x.ExamPracticeSectionId == request.ExamPracticeSectionId)
                                                                                      .FirstOrDefaultAsync(x => x.ExamPracticeResultId == request.ObjectResultId, cancellationToken);
            if (examPracticeSectionResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPracticeSectionResult));
                return methodResult;
            }

            var flatAnswers = ExtractConfigAnswers(examPracticeSectionResult.ExamPracticeAnswers);
            var answerById = flatAnswers
                .GroupBy(a => a.Id)
                .ToDictionary(g => g.Key, g => g.First()); // nếu trùng lấy cái đầu tiên

            var questionIds = await GetChildQuestionIdsAsync(examPracticeSectionResult.ExamPracticeSectionId, cancellationToken);
            if (!questionIds.Any())
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = new List<SubQuestionModel>();
                return methodResult;
            }

            var questions = await _questionRepository.Queryable.AsNoTracking()
                                                     .WhereBulkContains(questionIds, x => x.Id)
                                                     .OrderBy(x => x.CreatedDate)
                                                     .ToListAsync(cancellationToken);

            var listSubQuestion = new List<SubQuestionModel>();
            foreach (var item in questions)
            {
                var subQuestions = GetConfigQuestion(item.Config, item.QuestionType).ToList(); // List SubQuestion với IsExact = true // 3
                if (subQuestions.Count == 0)
                {
                    continue;
                }
                if (item.QuestionType == EnumQuestionType.CheckListV1)
                {
                    MarkChecklistProgress(item, subQuestions, flatAnswers);
                }
                ApplyMetadataAndStatus(examPracticeSectionResult.Status, item, subQuestions, answerById);
                listSubQuestion.AddRange(subQuestions);
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = listSubQuestion;
            return methodResult;
        }

        private static void MarkChecklistProgress(Question question, List<SubQuestionModel> subQuestions, List<ConfigAnswer> flatAnswers)
        {
            var conf = question.Config.Deserialize<CheckListQuestionV1>();
            var validIds = conf?.Answers?.Where(x => x.Id.HasValue).Select(a => a.Id!.Value).ToHashSet() ?? new HashSet<Guid>();

            // Đếm số câu trả lời hợp lệ map được theo Id
            var answeredCount = flatAnswers.Count(a => validIds.Contains(a.Id));

            // Đánh dấu PROCESS cho N sub đầu
            var upto = Math.Min(answeredCount, subQuestions.Count);
            for (var i = 0; i < upto; i++)
            {
                subQuestions[i].Status = EnumCorrectStatus.Process;
            }
        }

        private static List<ConfigAnswer> ExtractConfigAnswers(IEnumerable<ExamPracticeAnswer> answers)
        {
            return answers
                .Select(x => GetConfigAnswer(x.Answer))
                .Where(cfg => cfg != null && cfg.Answers != null && cfg.Answers.Any())
                .SelectMany(cfg => cfg!.Answers)
                .ToList();
        }

        private static void ApplyMetadataAndStatus(EnumResultStatus sectionStatus, Question q, List<SubQuestionModel> subs, IReadOnlyDictionary<Guid, ConfigAnswer> answerById)
        {
            // SubQuestionIndexs có thể null/thiếu index => an toàn chỉ số
            var hasIndexes = q.SubQuestionIndexs != null && q.SubQuestionIndexs.Count >= subs.Count;

            for (var i = 0; i < subs.Count; i++)
            {
                var sub = subs[i];

                sub.QuestionId = q.Id;
                sub.IndexSubQuestion = hasIndexes ? q.SubQuestionIndexs![i] : 0;

                // Tìm câu trả lời theo Id sub
                answerById.TryGetValue(sub.Id, out var cfgAnswer);
                var isExact = cfgAnswer?.IsExact;

                if (sectionStatus == EnumResultStatus.Done)
                {
                    sub.Status = (isExact.HasValue && isExact.Value)
                        ? EnumCorrectStatus.Correct
                        : EnumCorrectStatus.Fail;
                    continue;
                }

                if (sub.Status == EnumCorrectStatus.Process)
                {
                    // Đã set PROCESS từ bước checklist, giữ nguyên
                    continue;
                }

                if (q.QuestionType != EnumQuestionType.CheckListV1)
                {
                    var hasAnsweredSignal =
                        !string.IsNullOrEmpty(cfgAnswer?.Content) ||
                        !string.IsNullOrEmpty(cfgAnswer?.Key) ||
                        isExact.HasValue; // giữ semantics cũ

                    sub.Status = hasAnsweredSignal ? EnumCorrectStatus.Process : EnumCorrectStatus.New;
                }
                // Với CheckListV1 và chưa PROCESS, giữ nguyên status mặc định.
            }
        }

        /// <summary>
        /// Lấy tất cả QuestionId thuộc các section con của Section cha.
        /// </summary>
        private async Task<List<Guid>> GetChildQuestionIdsAsync(Guid parentSectionId, CancellationToken ct)
        {
            // Note: AsNoTracking vì chỉ đọc
            return await _examPracticeSectionRepository.Queryable
                .AsNoTracking()
                .Where(x => x.ParentExamPracticeSectionId == parentSectionId)
                .SelectMany(x => x.Questions.Select(q => q.Id))
                .ToListAsync(ct);
        }

        private static MultipleChoiceAnswerV1? GetConfigAnswer(object? answer)
        {
            return answer.Deserialize<MultipleChoiceAnswerV1>();
        }

        public static IList<SubQuestionModel> GetConfigQuestion(object? config, EnumQuestionType questionType)
        {
            switch (questionType)
            {
                case EnumQuestionType.MultichoiceV1:
                    var multichoiceV1 = config.Deserialize<MultipleChoiceQuestionV1>();
                    var subQuestionMultichoices = multichoiceV1?.Answers.Select(x => new SubQuestionModel { Id = x.Id ?? Guid.Empty, QuestionType = questionType }).ToList();
                    if (subQuestionMultichoices == null)
                    {
                        return new List<SubQuestionModel>();
                    }
                    return subQuestionMultichoices;

                case EnumQuestionType.YesNoNotGivenDropDown:
                    var yesNoNotGivenDropDown = config.Deserialize<MatchingTaskQuestion>();
                    var subQuestionYesNoNotGivens = yesNoNotGivenDropDown?.Answers.Select(x => new SubQuestionModel { Id = x.Id ?? Guid.Empty, QuestionType = questionType }).ToList();
                    if (subQuestionYesNoNotGivens == null)
                    {
                        return new List<SubQuestionModel>();
                    }
                    return subQuestionYesNoNotGivens;

                case EnumQuestionType.TrueFalseNotGivenDropDown:
                    var trueFalseNotGivenDropDown = config.Deserialize<MatchingTaskQuestion>();
                    var subQuestionTrueFalseNotGivens = trueFalseNotGivenDropDown?.Answers.Select(x => new SubQuestionModel { Id = x.Id ?? Guid.Empty, QuestionType = questionType }).ToList();
                    if (subQuestionTrueFalseNotGivens == null)
                    {
                        return new List<SubQuestionModel>();
                    }
                    return subQuestionTrueFalseNotGivens;

                case EnumQuestionType.MapLabelingDropDown:
                    var mapLabelingDropDown = config.Deserialize<MatchingTaskQuestion>();
                    var subQuestionMapLabelings = mapLabelingDropDown?.Answers.Select(x => new SubQuestionModel { Id = x.Id ?? Guid.Empty, QuestionType = questionType }).ToList();
                    if (subQuestionMapLabelings == null)
                    {
                        return new List<SubQuestionModel>();
                    }
                    return subQuestionMapLabelings;

                case EnumQuestionType.SummaryCompletionDropDown:
                    var summaryCompletionDropDown = config.Deserialize<MatchingTaskQuestion>();
                    var subQuestionSummaryCompletions = summaryCompletionDropDown?.Answers.Select(x => new SubQuestionModel { Id = x.Id ?? Guid.Empty, QuestionType = questionType }).ToList();
                    if (subQuestionSummaryCompletions == null)
                    {
                        return new List<SubQuestionModel>();
                    }
                    return subQuestionSummaryCompletions;

                case EnumQuestionType.MatchingParagraphInfo:
                    var matchingTask = config.Deserialize<MatchingTaskQuestion>();
                    var subQuestionMatchingParagraphs = matchingTask?.Answers.Select(x => new SubQuestionModel { Id = x.Id ?? Guid.Empty, QuestionType = questionType }).ToList();
                    if (subQuestionMatchingParagraphs == null)
                    {
                        return new List<SubQuestionModel>();
                    }
                    return subQuestionMatchingParagraphs;

                case EnumQuestionType.MatchingHeading:
                    var matchingHeading = config.Deserialize<MatchingTaskQuestion>();
                    var subQuestionMatchingHeadings = matchingHeading?.Answers.Select(x => new SubQuestionModel { Id = x.Id ?? Guid.Empty, QuestionType = questionType }).ToList();
                    if (subQuestionMatchingHeadings == null)
                    {
                        return new List<SubQuestionModel>();
                    }
                    return subQuestionMatchingHeadings;

                case EnumQuestionType.CheckListV1:
                    var checkList = config.Deserialize<CheckListQuestionV1>();
                    var subQuestionCheckLists = checkList?.Answers.Where(x => x.IsCorrect.HasValue && x.IsCorrect.Value).Select(x => new SubQuestionModel { Id = x.Id ?? Guid.Empty, QuestionType = questionType }).ToList();
                    if (subQuestionCheckLists == null)
                    {
                        return new List<SubQuestionModel>();
                    }
                    return subQuestionCheckLists;

                case EnumQuestionType.SummaryCompletionGapFill:
                    var summaryCompletionGapFill = config.Deserialize<CheckListQuestionV1>();
                    var subQuestionSummaryCompletionGapFills = summaryCompletionGapFill?.Answers.Select(x => new SubQuestionModel { Id = x.Id ?? Guid.Empty, QuestionType = questionType }).ToList();
                    if (subQuestionSummaryCompletionGapFills == null)
                    {
                        return new List<SubQuestionModel>();
                    }
                    return subQuestionSummaryCompletionGapFills;

                case EnumQuestionType.CompletionDiagrams:
                    var completionDiagrams = config.Deserialize<CheckListQuestionV1>();
                    var subQuestionCompletionDiagrams = completionDiagrams?.Answers.Select(x => new SubQuestionModel { Id = x.Id ?? Guid.Empty, QuestionType = questionType }).ToList();
                    if (subQuestionCompletionDiagrams == null)
                    {
                        return new List<SubQuestionModel>();
                    }
                    return subQuestionCompletionDiagrams;

                case EnumQuestionType.TableCompletion:
                    var tableCompletion = config.Deserialize<TableCompletionQuestion>();
                    var subQuestionTableCompletions = tableCompletion?.Answers.Select(x => new SubQuestionModel { Id = x.Id ?? Guid.Empty, QuestionType = questionType }).ToList();
                    if (subQuestionTableCompletions == null)
                    {
                        return new List<SubQuestionModel>();
                    }
                    return subQuestionTableCompletions;

                case EnumQuestionType.FlowChartCompletion:
                    var flowChartCompletion = config.Deserialize<CheckListQuestionV1>();
                    var subQuestionFlowChartCompletions = flowChartCompletion?.Answers.Select(x => new SubQuestionModel { Id = x.Id ?? Guid.Empty, QuestionType = questionType }).ToList();
                    if (subQuestionFlowChartCompletions == null)
                    {
                        return new List<SubQuestionModel>();
                    }
                    return subQuestionFlowChartCompletions;

                default:
                    return new List<SubQuestionModel>();
            }
        }
    }
}
