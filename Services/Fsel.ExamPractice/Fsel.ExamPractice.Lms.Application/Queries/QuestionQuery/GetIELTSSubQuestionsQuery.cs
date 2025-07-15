// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Queries.QuestionQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
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

            var examPracticeSectionResult = await _examPracticeSectionResultRepository.Queryable.Include(x => x.ExamPracticeAnswers).FirstOrDefaultAsync(x => x.ExamPracticeSectionId == request.ExamPracticeSectionId && x.ExamPracticeResultId == request.ObjectResultId, cancellationToken);
            if (examPracticeSectionResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPracticeSectionResult));
                return methodResult;
            }

            var configAnswers = examPracticeSectionResult.ExamPracticeAnswers
                                                .Select(x => GetConfigAnswer(x.Answer))
                                                .Where(x => x != null && x.Answers != null && x.Answers.Any())
                                                .SelectMany(x => x!.Answers)
                                                .ToList();

            var questionIds = await _examPracticeSectionRepository.Queryable.Where(x => x.ParentExamPracticeSectionId == examPracticeSectionResult.ExamPracticeSectionId)
                                                        .SelectMany(x => x.Questions)
                                                        .Select(x => x.Id)
                                                        .ToListAsync(cancellationToken);
            var listSubQuestion = new List<SubQuestionModel>();
            var questions = await _questionRepository.Queryable.WhereBulkContains(questionIds, x => x.Id).OrderBy(x => x.CreatedDate).ToListAsync(cancellationToken);
            foreach (var item in questions)
            {
                var subQuestions = GetConfigQuestion(item.Config, item.QuestionType); // List SubQuestion với IsExact = true // 3
                if (item.QuestionType == EnumQuestionType.CheckListV1)
                {
                    var answerConfigIds = item.Config.Deserialize<CheckListQuestionV1>()?.Answers.Select(x => x.Id).ToList();
                    var answers = configAnswers.Where(x => answerConfigIds != null && answerConfigIds.Contains(x.Id)).ToList();
                    subQuestions = subQuestions.Select((x, index) =>
                    {
                        if (answers.Count > index)
                        {
                            x.Status = EnumCorrectStatus.Process;
                        }
                        return x;
                    }).ToList();
                }
                foreach (var subQuestion in subQuestions)
                {
                    var configAnswer = configAnswers.FirstOrDefault(x => x.Id == subQuestion.Id);
                    var isExact = configAnswer?.IsExact;
                    subQuestion.QuestionId = item.Id;
                    subQuestion.IndexSubQuestion = item.SubQuestionIndexs?[subQuestions.IndexOf(subQuestion)] ?? 0;
                    if (examPracticeSectionResult.Status == EnumResultStatus.Done)
                    {
                        subQuestion.Status = isExact.HasValue && isExact.Value ? EnumCorrectStatus.Correct : EnumCorrectStatus.Fail;
                    }
                    else if (subQuestion.Status == EnumCorrectStatus.Process)
                    {
                        continue;
                    }
                    else if (item.QuestionType != EnumQuestionType.CheckListV1)
                    {
                        subQuestion.Status = !(string.IsNullOrEmpty(configAnswer?.Content) && string.IsNullOrEmpty(configAnswer?.Key)) || isExact.HasValue ? EnumCorrectStatus.Process : EnumCorrectStatus.New;
                    }
                }
                listSubQuestion.AddRange(subQuestions);
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = listSubQuestion;
            return methodResult;
        }

        private static MultipleChoiceAnswerV1? GetConfigAnswer(object? answer)
        {
            return answer.Deserialize<MultipleChoiceAnswerV1>();
        }

        public IList<SubQuestionModel> GetConfigQuestion(object? config, EnumQuestionType questionType)
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
