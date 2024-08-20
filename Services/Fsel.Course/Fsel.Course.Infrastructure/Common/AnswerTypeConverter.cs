// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using System;
    using System.Linq;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers;
    using Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers.V1i1;
    using Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions;
    using Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;

    public class AnswerTypeConverter
    {
        private readonly LinQAnswerHelper _linQAnswerHelper;

        public AnswerTypeConverter(LinQAnswerHelper linQAnswerHelper)
        {
            _linQAnswerHelper = linQAnswerHelper;
        }

        public (object?, int, bool, bool) GetTotalCorrectByAnswerType(object? configAnswer, object? configOldAnswer, Question question, bool isTryAgain = false, bool isSubmit = false, bool isMandatoryAnswer = false)
        {
            int totalCorrect;
            bool isAnswerMissing;
            bool isAnswered;
            switch (question?.QuestionType)
            {
                case EnumQuestionType.Multichoice:
                case EnumQuestionType.Dropdown:
                case EnumQuestionType.Checklist:
                    (totalCorrect, isAnswerMissing, isAnswered) = HandleCheckListAnswer(ref configAnswer, configOldAnswer.Deserialize<MultipleChoiceAnswer>(), question.Config.Deserialize<MultipleChoiceQuestion>(), isTryAgain, isSubmit, isMandatoryAnswer);
                    break;

                case EnumQuestionType.Listing:
                    (totalCorrect, isAnswerMissing, isAnswered) = HandleListingAnswer(ref configAnswer, configOldAnswer.Deserialize<ListingAnswer>(), question.Config.Deserialize<ListingQuestion>(), isTryAgain, isSubmit, isMandatoryAnswer);
                    break;

                case EnumQuestionType.DragAndDropPicture:
                case EnumQuestionType.MatchingType1:
                case EnumQuestionType.MatchingType2:
                    (totalCorrect, isAnswerMissing, isAnswered) = HandleMaschingTypeAnswer(ref configAnswer, configOldAnswer.Deserialize<MatchingTypeAnswer>(), question.Config.Deserialize<MatchingTypeQuestion>(), isTryAgain, isSubmit, isMandatoryAnswer);
                    break;

                case EnumQuestionType.ShortAnswerWordBase:
                    (totalCorrect, isAnswerMissing, isAnswered) = HandleShortAnswerWordBase(ref configAnswer, configOldAnswer.Deserialize<ShortAnswerWordBaseAnswer>(), question.Config.Deserialize<ShortAnswerQuestionWordBaseQuestion>(), isTryAgain, isSubmit, isMandatoryAnswer);
                    break;

                case EnumQuestionType.ShortAnswerWordCount:
                    (totalCorrect, isAnswerMissing, isAnswered) = HandleShortAnswerWordCount(ref configAnswer, configOldAnswer.Deserialize<ShortAnswerWordCountBaseAnswer>(), question.Config.Deserialize<ShortAnswerQuestionWordCountBaseQuestion>(), isTryAgain, isSubmit, isMandatoryAnswer);
                    break;

                case EnumQuestionType.GapFillScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByQuestion:
                    (totalCorrect, isAnswerMissing, isAnswered) = HandleGapFillBySubAnswer(ref configAnswer, configOldAnswer.Deserialize<GapFillAnswer>(), question.Config.Deserialize<GapFillQuestion>(), isTryAgain, isSubmit, isMandatoryAnswer);
                    break;

                case EnumQuestionType.GapFillWordBankScoreByGap:
                case EnumQuestionType.GapFillScoreByGap:
                    (totalCorrect, isAnswerMissing, isAnswered) = HandleGapFillGapAnswer(ref configAnswer, configOldAnswer.Deserialize<GapFillAnswer>(), question.Config.Deserialize<GapFillQuestion>(), isTryAgain, isSubmit, isMandatoryAnswer);
                    break;

                case EnumQuestionType.DragAndDropSentenceOrder:
                    (totalCorrect, isAnswerMissing, isAnswered) = HandleDragDropOrderAnswer(ref configAnswer, configOldAnswer.Deserialize<DragAndDropSentenceOrderAnswer>(), question.Config.Deserialize<DragAndDropSentenceOrderQuestion>(), isTryAgain, isSubmit, isMandatoryAnswer);
                    break;

                case EnumQuestionType.DragAndDropListSentenceOrder:
                    (totalCorrect, isAnswerMissing, isAnswered) = HandleAnswer(ref configAnswer, configOldAnswer.Deserialize<DragAndDropListSentenceOrderAnswer>(), question.Config.Deserialize<DragAndDropListSentenceOrderQuestion>(), isTryAgain, isSubmit, isMandatoryAnswer);
                    break;

                case EnumQuestionType.MultipleOptionSentenceCompletion:
                    (totalCorrect, isAnswerMissing, isAnswered) = HandleMultipleOptionAnswer(ref configAnswer, configOldAnswer.Deserialize<MultipleOptionSentenceCompletionAnswer>(), question.Config.Deserialize<MultipleOptionSentenceCompletionQuestion>(), isTryAgain, isSubmit, isMandatoryAnswer);
                    break;

                case EnumQuestionType.ExercisePreparation:
                    totalCorrect = question.CorrectTotal;
                    (isAnswerMissing, isAnswered) = (false, true);
                    break;

                //V1

                case EnumQuestionType.MultichoiceV1:
                    (totalCorrect, isAnswerMissing, isAnswered) = HandleAnswerMultipleChoiceV1(ref configAnswer, configOldAnswer.Deserialize<MultipleChoiceAnswerV1>(), question.Config.Deserialize<MultipleChoiceQuestionV1>(), isTryAgain, isSubmit, isMandatoryAnswer);
                    break;

                case EnumQuestionType.CheckListV1:
                    (totalCorrect, isAnswerMissing, isAnswered) = HandleAnswerCheckListV1(ref configAnswer, configOldAnswer.Deserialize<MultipleChoiceAnswerV1>(), question.Config.Deserialize<CheckListQuestionV1>(), isTryAgain, isSubmit, isMandatoryAnswer);
                    break;

                case EnumQuestionType.SummaryCompletionGapFill:
                case EnumQuestionType.CompletionDiagrams:
                case EnumQuestionType.FlowChartCompletion:
                    (totalCorrect, isAnswerMissing, isAnswered) = HandleAnswerSummaryCompletionGapFill(ref configAnswer, configOldAnswer.Deserialize<MultipleChoiceAnswerV1>(), question.Config.Deserialize<CheckListQuestionV1>(), isTryAgain, isSubmit, isMandatoryAnswer);
                    break;

                case EnumQuestionType.YesNoNotGivenDropDown:
                case EnumQuestionType.TrueFalseNotGivenDropDown:
                case EnumQuestionType.MapLabelingDropDown:
                case EnumQuestionType.SummaryCompletionDropDown:
                case EnumQuestionType.MatchingParagraphInfo:
                case EnumQuestionType.MatchingHeading:
                    (totalCorrect, isAnswerMissing, isAnswered) = HandleAnswerMatchingTask(ref configAnswer, configOldAnswer.Deserialize<MultipleChoiceAnswerV1>(), question.Config.Deserialize<MatchingTaskQuestion>(), isTryAgain, isSubmit, isMandatoryAnswer);
                    break;

                //(totalCorrect, isAnswerMissing, isAnswered) = HandleAnswerFlowChartCompletion(ref configAnswer, configOldAnswer.Deserialize<MultipleChoiceAnswerV1>(), question.Config.Deserialize<FlowChartCompletionQuestion>(), isTryAgain, isSubmit, isMandatoryAnswer);
                //break;

                case EnumQuestionType.TableCompletion:
                    (totalCorrect, isAnswerMissing, isAnswered) = HandleAnswerTableCompletion(ref configAnswer, configOldAnswer.Deserialize<MultipleChoiceAnswerV1>(), question.Config.Deserialize<TableCompletionQuestion>(), isTryAgain, isSubmit, isMandatoryAnswer);
                    break;

                default:
                    return default;
            }
            return (configAnswer, totalCorrect, isAnswerMissing, isAnswered);
        }

        public int GetTotalCorrectByAnswerType(Question? question, object? configAnswer)
        {
            switch (question?.QuestionType)
            {
                case EnumQuestionType.MultichoiceV1:
                case EnumQuestionType.CheckListV1:
                case EnumQuestionType.SummaryCompletionGapFill:
                case EnumQuestionType.CompletionDiagrams:
                case EnumQuestionType.YesNoNotGivenDropDown:
                case EnumQuestionType.TrueFalseNotGivenDropDown:
                case EnumQuestionType.MapLabelingDropDown:
                case EnumQuestionType.SummaryCompletionDropDown:
                case EnumQuestionType.MatchingParagraphInfo:
                case EnumQuestionType.MatchingHeading:
                case EnumQuestionType.FlowChartCompletion:
                case EnumQuestionType.TableCompletion:
                    return configAnswer.Deserialize<MultipleChoiceAnswerV1>()?.Answers.Count ?? default;

                default:
                    return default;
            }
        }

        public object? AnswerTypeConverterObject(object? configAnswer, EnumQuestionType type, bool isShowSubStatus, EnumResultStatus status, bool isDisableAnswer = true)
        {
            object? result;
            switch (type)
            {
                case EnumQuestionType.Multichoice:
                case EnumQuestionType.Dropdown:
                case EnumQuestionType.Checklist:
                    var multichoice = configAnswer.Deserialize<MultipleChoiceAnswer>();
                    result = GetAnswer(multichoice, isShowSubStatus, status, isDisableAnswer);
                    break;

                case EnumQuestionType.Listing:
                    var listingQuestion = configAnswer.Deserialize<ListingAnswer>();
                    result = GetAnswer(listingQuestion, isShowSubStatus, status, isDisableAnswer);
                    break;

                case EnumQuestionType.MatchingType1:
                case EnumQuestionType.MatchingType2:
                case EnumQuestionType.DragAndDropPicture:
                    var matchingTypeQuestion = configAnswer.Deserialize<MatchingTypeAnswer>();
                    result = GetAnswer(matchingTypeQuestion, isShowSubStatus, status, isDisableAnswer);
                    break;

                case EnumQuestionType.ShortAnswerWordBase:
                    var shortAnswerQuestionWordBaseQuestion = configAnswer.Deserialize<ShortAnswerWordBaseAnswer>();
                    result = GetAnswer(shortAnswerQuestionWordBaseQuestion, isShowSubStatus, status, isDisableAnswer);
                    break;

                case EnumQuestionType.ShortAnswerWordCount:
                    var shortAnswerWordCount = configAnswer.Deserialize<ShortAnswerWordCountBaseAnswer>();
                    result = GetAnswer(shortAnswerWordCount, isShowSubStatus, status, isDisableAnswer);
                    break;

                case EnumQuestionType.GapFillScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByGap:
                case EnumQuestionType.GapFillScoreByGap:
                    var gapFillQuestion = configAnswer.Deserialize<GapFillAnswer>();
                    result = GetAnswer(gapFillQuestion, isShowSubStatus, status, isDisableAnswer);
                    break;

                case EnumQuestionType.DragAndDropSentenceOrder:
                    var dragAndDropSentenceOrderQuestion = configAnswer.Deserialize<DragAndDropSentenceOrderAnswer>();
                    result = GetAnswer(dragAndDropSentenceOrderQuestion, isShowSubStatus, status, isDisableAnswer);
                    break;

                case EnumQuestionType.DragAndDropListSentenceOrder:
                    var dragAndDropSentenceListOrderQuestion = configAnswer.Deserialize<DragAndDropListSentenceOrderAnswer>();
                    result = GetAnswer(dragAndDropSentenceListOrderQuestion, isShowSubStatus, status, isDisableAnswer);
                    break;

                case EnumQuestionType.MultipleOptionSentenceCompletion:
                    var multipleOption = configAnswer.Deserialize<MultipleOptionSentenceCompletionAnswer>();
                    result = GetAnswer(multipleOption, isShowSubStatus, status, isDisableAnswer);
                    break;

                case EnumQuestionType.ExercisePreparation:
                    var exercisePreparation = configAnswer.Deserialize<ExercisePreparationQuestion>();
                    result = exercisePreparation;
                    break;

                //V1

                case EnumQuestionType.MultichoiceV1:
                case EnumQuestionType.CheckListV1:
                case EnumQuestionType.SummaryCompletionGapFill:
                case EnumQuestionType.CompletionDiagrams:
                case EnumQuestionType.YesNoNotGivenDropDown:
                case EnumQuestionType.TrueFalseNotGivenDropDown:
                case EnumQuestionType.MapLabelingDropDown:
                case EnumQuestionType.SummaryCompletionDropDown:
                case EnumQuestionType.MatchingParagraphInfo:
                case EnumQuestionType.MatchingHeading:
                case EnumQuestionType.FlowChartCompletion:
                case EnumQuestionType.TableCompletion:
                    var matchingTaskQuestion = configAnswer.Deserialize<MultipleChoiceAnswerV1>();
                    result = GetAnswer(matchingTaskQuestion, isShowSubStatus, status, isDisableAnswer);
                    break;

                default:
                    throw new ArgumentException("Invalid question type");
            }

            return result;
        }

        private static bool? IsDisableAnswers(EnumResultStatus status, bool isFirstSubmit, bool? isExact, bool isShowSubStatus, bool isDisableAnswer)
        {
            return isDisableAnswer && ((status == EnumResultStatus.Process && isFirstSubmit && isExact == true) || isShowSubStatus) ? isExact : default;
        }

        private static object? GetAnswer(MultipleOptionSentenceCompletionAnswer? data, bool isShowSubStatus, EnumResultStatus status, bool isDisableAnswer)
        {
            if (data != null && data.Answers != null && status != EnumResultStatus.Done)
            {
                foreach (var item in data.Answers)
                {
                    item.IsExact = IsDisableAnswers(status, item.IsFirstSubmit, item.IsExact, isShowSubStatus, isDisableAnswer);
                }
            }
            return data;
        }

        private static object? GetAnswer(MultipleChoiceAnswerV1? data, bool isShowSubStatus, EnumResultStatus status, bool isDisableAnswer)
        {
            if (data != null && data.Answers != null && status != EnumResultStatus.Done)
            {
                foreach (var item in data.Answers)
                {
                    item.IsExact = IsDisableAnswers(status, item.IsFirstSubmit, item.IsExact, isShowSubStatus, isDisableAnswer);
                }
            }
            return data;
        }

        private static object? GetAnswer(DragAndDropSentenceOrderAnswer? data, bool isShowSubStatus, EnumResultStatus status, bool isDisableAnswer)
        {
            if (data != null && data.Answers != null && status != EnumResultStatus.Done)
            {
                foreach (var item in data.Answers)
                {
                    item.IsExact = IsDisableAnswers(status, item.IsFirstSubmit, item.IsExact, isShowSubStatus, isDisableAnswer);
                }
            }
            return data;
        }

        private static object? GetAnswer(DragAndDropListSentenceOrderAnswer? data, bool isShowSubStatus, EnumResultStatus status, bool isDisableAnswer)
        {
            if (data != null && data.Answers != null && status != EnumResultStatus.Done)
            {
                foreach (var item in data.Answers)
                {
                    item.IsExact = IsDisableAnswers(status, item.IsFirstSubmit, item.IsExact, isShowSubStatus, isDisableAnswer);
                }
            }
            return data;
        }

        private static object? GetAnswer(ShortAnswerWordBaseAnswer? data, bool isShowSubStatus, EnumResultStatus status, bool isDisableAnswer)
        {
            if (data != null && status != EnumResultStatus.Done)
            {
                data.IsExact = IsDisableAnswers(status, data.IsFirstSubmit, data.IsExact, isShowSubStatus, isDisableAnswer);
            }
            return data;
        }

        private static object? GetAnswer(ListingAnswer? data, bool isShowSubStatus, EnumResultStatus status, bool isDisableAnswer)
        {
            if (data != null && status != EnumResultStatus.Done)
            {
                data.IsExact = IsDisableAnswers(status, data.IsFirstSubmit, data.IsExact, isShowSubStatus, isDisableAnswer);
            }
            return data;
        }

        private static object? GetAnswer(MultipleChoiceAnswer? data, bool isShowSubStatus, EnumResultStatus status, bool isDisableAnswer)
        {
            if (data != null && data.Answers != null && status != EnumResultStatus.Done)
            {
                foreach (var item in data.Answers)
                {
                    item.IsExact = IsDisableAnswers(status, item.IsFirstSubmit, item.IsExact, isShowSubStatus, isDisableAnswer);
                }
            }
            return data;
        }

        private static object? GetAnswer(ShortAnswerWordCountBaseAnswer? data, bool isShowSubStatus, EnumResultStatus status, bool isDisableAnswer)
        {
            if (data != null && status != EnumResultStatus.Done)
            {
                data.IsExact = IsDisableAnswers(status, data.IsFirstSubmit, data.IsExact, isShowSubStatus, isDisableAnswer);
            }
            return data;
        }

        private static object? GetAnswer(MatchingTypeAnswer? data, bool isShowSubStatus, EnumResultStatus status, bool isDisableAnswer)
        {
            if (data != null && data.Answers != null && status != EnumResultStatus.Done)
            {
                foreach (var item in data.Answers)
                {
                    item.IsExact = IsDisableAnswers(status, item.IsFirstSubmit, item.IsExact, isShowSubStatus, isDisableAnswer);
                }
            }
            return data;
        }

        private static object? GetAnswer(GapFillAnswer? data, bool isShowSubStatus, EnumResultStatus status, bool isDisableAnswer)
        {
            if (data != null && data.Answers != null && status != EnumResultStatus.Done)
            {
                foreach (var item in data.Answers)
                {
                    item.IsExacts = item.IsExacts?.Select((x, index) =>
                    {
                        return IsDisableAnswers(status, item.IsFirstSubmits != null && item.IsFirstSubmits[index], x, isShowSubStatus, isDisableAnswer);
                    }).ToList();
                }
            }
            return data;
        }

        private bool IsAnswerMissing(object? dataAnswer, object? dataQuestion, EnumQuestionType type, bool isSubmit = false, bool isMandatoryAnswer = false)
        {
            if (!isMandatoryAnswer || !isSubmit)
            {
                return false;
            }
            switch (type)
            {
                case EnumQuestionType.Multichoice:
                case EnumQuestionType.Dropdown:
                case EnumQuestionType.Checklist:
                    return (!_linQAnswerHelper.CheckAnswerCount(dataAnswer, dataQuestion) || !_linQAnswerHelper.IsNullOrEmptyData(dataAnswer, nameof(MultipleChoiceAnswers.IsChecked), false));

                case EnumQuestionType.Listing:
                    return _linQAnswerHelper.IsNullOrEmptyData(dataAnswer);

                case EnumQuestionType.DragAndDropPicture:
                case EnumQuestionType.MatchingType1:
                case EnumQuestionType.MatchingType2:
                    return (!_linQAnswerHelper.CheckAnswerCount(dataAnswer, dataQuestion) || _linQAnswerHelper.IsNullOrEmptyData(dataAnswer, nameof(MatchingTypeAnswers.ToId)));

                case EnumQuestionType.ShortAnswerWordBase:
                case EnumQuestionType.ShortAnswerWordCount:
                    return string.IsNullOrEmpty(dataAnswer?.ToString());

                case EnumQuestionType.GapFillScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByGap:
                case EnumQuestionType.GapFillScoreByGap:
                case EnumQuestionType.DragAndDropSentenceOrder:
                    return (!_linQAnswerHelper.CheckAnswerCount(dataAnswer, dataQuestion) || _linQAnswerHelper.IsNullOrEmptyData(dataAnswer, nameof(GapFillAnswers.Answer)));

                case EnumQuestionType.DragAndDropListSentenceOrder:
                    return !_linQAnswerHelper.CheckAnswerCount(dataAnswer, dataQuestion);

                case EnumQuestionType.MultipleOptionSentenceCompletion:
                    return _linQAnswerHelper.IsNullOrEmptyData(dataAnswer, nameof(MultipleOptionSentenceCompletionAnswers.AnswerId));

                case EnumQuestionType.MultichoiceV1:
                case EnumQuestionType.CheckListV1:
                case EnumQuestionType.SummaryCompletionGapFill:
                case EnumQuestionType.CompletionDiagrams:
                case EnumQuestionType.YesNoNotGivenDropDown:
                case EnumQuestionType.TrueFalseNotGivenDropDown:
                case EnumQuestionType.MapLabelingDropDown:
                case EnumQuestionType.SummaryCompletionDropDown:
                case EnumQuestionType.MatchingParagraphInfo:
                case EnumQuestionType.MatchingHeading:
                case EnumQuestionType.FlowChartCompletion:
                case EnumQuestionType.TableCompletion:
                    return _linQAnswerHelper.IsDuplicateAnswerId(dataAnswer, nameof(ConfigAnswer.Id));

                default:
                    return default;
            }
        }

        #region Handle Answer

        private (int, bool, bool) HandleCheckListAnswer(ref object? configAnswer, MultipleChoiceAnswer? dataOldAnswer, MultipleChoiceQuestion? dataQuestion, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            int number = 0;
            var dataAnswer = configAnswer.Deserialize<MultipleChoiceAnswer>();
            bool isAnswerMissing = IsAnswerMissing(dataAnswer?.Answers, dataQuestion?.Contents, EnumQuestionType.Multichoice, isSubmit, isMandatoryAnswer);
            if (dataQuestion?.Contents == null || ((dataAnswer == null || dataAnswer.Answers == null) || !dataAnswer.Answers.Any()) || (isMandatoryAnswer && isAnswerMissing))
            {
                configAnswer = dataAnswer;
                return (default, isAnswerMissing, false);
            }
            foreach (var item in dataAnswer.Answers)
            {
                if (!item.IsChecked)
                {
                    item.IsExact = default;
                    continue;
                }

                item.IsExact = dataQuestion.Contents.Any(x => x.Id == item.Id && x.IsCorrect == item.IsChecked);
                number = item.IsExact == true ? ++number : --number;

                if (isTryAgain && dataOldAnswer != null && dataOldAnswer.Answers != null)
                {
                    var answer = dataOldAnswer.Answers.FirstOrDefault(x => x.Id == item.Id);
                    if (!(answer != null && answer.IsExact == true && answer.IsFirstSubmit))
                    {
                        item.IsFirstSubmit = false;
                    }
                }
            }
            configAnswer = dataAnswer;
            return (number > 0 ? number : default, isAnswerMissing, _linQAnswerHelper.IsNullOrEmptyData(dataAnswer.Answers, nameof(MultipleChoiceAnswers.IsChecked), false));
        }

        private (int, bool, bool) HandleListingAnswer(ref object? configAnswer, ListingAnswer? dataOldAnswer, ListingQuestion? dataQuestion, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            int number = 0;
            var dataAnswer = configAnswer.Deserialize<ListingAnswer>();
            bool isAnswerMissing = IsAnswerMissing(dataAnswer?.Answers, default, EnumQuestionType.Listing, isSubmit, isMandatoryAnswer);
            if (((dataAnswer == null || dataAnswer.Answers == null) || !dataAnswer.Answers.Any()) || (isMandatoryAnswer && isAnswerMissing))
            {
                configAnswer = dataAnswer;
                return (default, isAnswerMissing, false);
            }

            if (dataAnswer.Answers.Where(x => !string.IsNullOrEmpty(x.Trim())).Count() >= dataQuestion?.ExactWordCount)
            {
                dataAnswer.IsExact = true;
                number++;
            }
            else
            {
                dataAnswer.IsExact = false;
            }
            if (isTryAgain && dataOldAnswer?.Answers != null && !(dataOldAnswer.IsExact == true && dataOldAnswer.IsFirstSubmit))
            {
                dataAnswer.IsFirstSubmit = false;
            }
            configAnswer = dataAnswer;
            return (number, isAnswerMissing, _linQAnswerHelper.IsAnswerHaveData(dataAnswer.Answers));
        }

        private (int, bool, bool) HandleMaschingTypeAnswer(ref object? configAnswer, MatchingTypeAnswer? dataOldAnswer, MatchingTypeQuestion? dataQuestion, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<MatchingTypeAnswer>();
            int number = default;
            bool isAnswerMissing = IsAnswerMissing(dataAnswer?.Answers, dataQuestion?.Link, EnumQuestionType.MatchingType1, isSubmit, isMandatoryAnswer);
            if (((dataAnswer == null || dataAnswer.Answers == null) || !dataAnswer.Answers.Any()) || dataQuestion?.Link == null || (isMandatoryAnswer && isAnswerMissing))
            {
                configAnswer = dataAnswer;
                return (default, isAnswerMissing, false);
            }
            foreach (var item in dataAnswer.Answers)
            {
                var link = dataQuestion.Link.FirstOrDefault(x => x.FromId == item.FromId);
                if (link == null)
                {
                    item.IsExact = false;
                }
                else
                {
                    var textQuestion = dataQuestion.To?.FirstOrDefault(x => x.Id == link.ToId)?.Content.ReplaceWord();
                    var textAnswer = dataQuestion.To?.FirstOrDefault(x => x.Id == item.ToId)?.Content.ReplaceWord();
                    var isExact = dataQuestion.Link.Any(x => x.FromId == item.FromId && (item.ToId.HasValue && x.ToId == item.ToId))
                        || textAnswer == textQuestion;

                    if (isExact)
                    {
                        number++;
                    }
                    item.IsExact = isExact;
                }
                if (isTryAgain && dataOldAnswer != null && dataOldAnswer.Answers != null)
                {
                    var answer = dataOldAnswer.Answers.FirstOrDefault(x => x.FromId == item.FromId);
                    if (!(answer != null && answer.IsExact == true && answer.IsFirstSubmit))
                    {
                        item.IsFirstSubmit = false;
                    }
                }
            }
            configAnswer = dataAnswer;
            return (number, isAnswerMissing, _linQAnswerHelper.IsAnswerHaveData(dataAnswer.Answers, nameof(MatchingTypeAnswers.ToId)));
        }

        private (int, bool, bool) HandleShortAnswerWordCount(ref object? configAnswer, ShortAnswerWordCountBaseAnswer? dataOldAnswer, ShortAnswerQuestionWordCountBaseQuestion? dataQuestion, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<ShortAnswerWordCountBaseAnswer>();
            int number = 0;
            bool isAnswerMissing = IsAnswerMissing(dataAnswer?.Answers, default, EnumQuestionType.ShortAnswerWordCount, isSubmit, isMandatoryAnswer);
            var isAnswered = string.IsNullOrEmpty(dataAnswer?.Answers);
            if ((dataAnswer == null || isAnswered) || (isMandatoryAnswer && isAnswerMissing))
            {
                configAnswer = dataAnswer;
                return (default, isAnswerMissing, false);
            }
            var exactWordCount = Shared.Helpers.StringHelper.CountWords(dataAnswer.Answers);
            if (exactWordCount >= dataQuestion?.ExactWordCount)
            {
                dataAnswer.IsExact = true;
                number++;
            }
            else
            {
                dataAnswer.IsExact = false;
            }
            if (isTryAgain && dataOldAnswer != null && dataOldAnswer.Answers != null && !(dataOldAnswer.IsExact == true && dataOldAnswer.IsFirstSubmit))
            {
                dataAnswer.IsFirstSubmit = false;
            }
            configAnswer = dataAnswer;
            return (number, isAnswerMissing, !isAnswered);
        }

        private (int, bool, bool) HandleShortAnswerWordBase(ref object? configAnswer, ShortAnswerWordBaseAnswer? dataOldAnswer, ShortAnswerQuestionWordBaseQuestion? dataQuestion, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<ShortAnswerWordBaseAnswer>();
            int number = 0;
            var isAnswered = string.IsNullOrEmpty(dataAnswer?.Answers);
            bool isAnswerMissing = IsAnswerMissing(dataAnswer?.Answers, dataQuestion?.Content, EnumQuestionType.ShortAnswerWordBase, isSubmit, isMandatoryAnswer);
            if ((dataAnswer == null || isAnswered) || dataQuestion?.Content == null || (isMandatoryAnswer && isAnswerMissing))
            {
                return (default, isAnswerMissing, false);
            }
            if (dataQuestion.Content.Any(p => _linQAnswerHelper.IsShortAnswer(p, dataAnswer.Answers)))
            {
                dataAnswer.IsExact = true;
                number++;
            }
            else
            {
                dataAnswer.IsExact = false;
            }
            if (isTryAgain && dataOldAnswer != null && dataOldAnswer.Answers != null && !(dataOldAnswer.IsExact == true && dataOldAnswer.IsFirstSubmit))
            {
                dataAnswer.IsFirstSubmit = false;
            }
            configAnswer = dataAnswer;
            return (number, isAnswerMissing, !isAnswered);
        }

        private (int, bool, bool) HandleGapFillBySubAnswer(ref object? configAnswer, GapFillAnswer? dataOldAnswer, GapFillQuestion? dataQuestion, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<GapFillAnswer>();
            int number = 0;
            bool isAnswerMissing = IsAnswerMissing(dataAnswer?.Answers, dataQuestion?.Contents, EnumQuestionType.GapFillScoreByQuestion, isSubmit, isMandatoryAnswer);
            if (((dataAnswer == null || dataAnswer.Answers == null) || !dataAnswer.Answers.Any()) || dataQuestion?.Contents == null || (isMandatoryAnswer && isAnswerMissing))
            {
                configAnswer = dataAnswer;
                return (default, isAnswerMissing, false);
            }
            foreach (var item in dataAnswer.Answers)
            {
                var question = dataQuestion.Contents.FirstOrDefault(c => c.Id == item.Id);
                if (question?.Words != null && item.Answer?.Any() == true)
                {
                    item.IsExacts = item.Answer.Select((word, index) => _linQAnswerHelper.CheckAnswer(question.Words, word, index)).ToList();
                    if (item.IsExacts.Count(x => x == true) == item.IsExacts.Count)
                    {
                        number++;
                    }
                    if (isTryAgain && dataOldAnswer != null && dataOldAnswer.Answers != null)
                    {
                        var answer = dataOldAnswer.Answers.FirstOrDefault(x => x.Id == item.Id);
                        if (answer != null && answer.IsFirstSubmits != null && answer.IsExacts != null)
                        {
                            var isFirstSubmit = new List<bool>();
                            var index = 0;
                            foreach (var data in answer.IsExacts)
                            {
                                if (answer.IsFirstSubmits[index] && data.HasValue && !data.Value)
                                {
                                    isFirstSubmit.Add(false);
                                }
                                else
                                {
                                    isFirstSubmit.Add(answer.IsFirstSubmits[index]);
                                }
                                index++;
                            }
                            item.IsFirstSubmits = isFirstSubmit;
                        }
                    }
                }
            }
            configAnswer = dataAnswer;
            return (number, isAnswerMissing, _linQAnswerHelper.IsAnswerHaveData(dataAnswer.Answers, nameof(GapFillAnswers.Answer)));
        }

        private (int, bool, bool) HandleGapFillGapAnswer(ref object? configAnswer, GapFillAnswer? dataOldAnswer, GapFillQuestion? dataQuestion, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<GapFillAnswer>();
            int number = 0;
            bool isAnswerMissing = IsAnswerMissing(dataAnswer?.Answers, dataQuestion?.Contents, EnumQuestionType.GapFillScoreByQuestion, isSubmit, isMandatoryAnswer);
            if (((dataAnswer == null || dataAnswer.Answers == null) || !dataAnswer.Answers.Any()) || dataQuestion?.Contents == null || (isMandatoryAnswer && isAnswerMissing))
            {
                configAnswer = dataAnswer;
                return (default, isAnswerMissing, false);
            }
            foreach (var item in dataAnswer.Answers)
            {
                var question = dataQuestion.Contents.FirstOrDefault(c => c.Id == item.Id);
                if (question != null && question.Words != null && question.Words.Any() && item.Answer != null && item.Answer.Any())
                {
                    item.IsExacts = item.Answer.Select((word, index) => _linQAnswerHelper.CheckAnswer(question.Words, word, index)).ToList();
                    number += item.IsExacts.Count(x => x == true);
                    if (isTryAgain && dataOldAnswer != null && dataOldAnswer.Answers != null)
                    {
                        var answer = dataOldAnswer.Answers.FirstOrDefault(x => x.Id == item.Id);
                        if (answer != null && answer.IsFirstSubmits != null && answer.IsExacts != null)
                        {
                            var isFirstSubmit = new List<bool>();
                            var index = 0;
                            foreach (var data in answer.IsExacts)
                            {
                                if (answer.IsFirstSubmits[index] && data.HasValue && !data.Value)
                                {
                                    isFirstSubmit.Add(false);
                                }
                                else
                                {
                                    isFirstSubmit.Add(answer.IsFirstSubmits[index]);
                                }
                                index++;
                            }
                            item.IsFirstSubmits = isFirstSubmit;
                        }
                    }
                }
            }
            configAnswer = dataAnswer;
            return (number, isAnswerMissing, _linQAnswerHelper.IsAnswerHaveData(dataAnswer.Answers, nameof(GapFillAnswers.Answer)));
        }

        private (int, bool, bool) HandleDragDropOrderAnswer(ref object? configAnswer, DragAndDropSentenceOrderAnswer? dataOldAnswer, DragAndDropSentenceOrderQuestion? dataQuestion, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<DragAndDropSentenceOrderAnswer>();
            int number = 0;
            bool isAnswerMissing = IsAnswerMissing(dataAnswer?.Answers, dataQuestion?.Contents, EnumQuestionType.DragAndDropSentenceOrder, isSubmit, isMandatoryAnswer);
            if (((dataAnswer == null || dataAnswer.Answers == null) || !dataAnswer.Answers.Any()) || dataQuestion?.Contents == null || (isMandatoryAnswer && isAnswerMissing))
            {
                configAnswer = dataAnswer;
                return (default, isAnswerMissing, false);
            }
            if (!isMandatoryAnswer || (isMandatoryAnswer && !isAnswerMissing))
            {
                foreach (var item in dataAnswer.Answers)
                {
                    var question = dataQuestion.Contents.FirstOrDefault(c => c.Id == item.Id);
                    if (question != null && item.Answer != null && question.Words != null && question.Words.Count > 0)
                    {
                        var content = question.Words.SequenceEqual(item.Answer);
                        if (content)
                        {
                            number++;
                        }
                        item.IsExact = content;
                    }
                    else
                    {
                        item.IsExact = false;
                    }
                    if (isTryAgain && dataOldAnswer != null && dataOldAnswer.Answers != null)
                    {
                        var answer = dataOldAnswer.Answers.FirstOrDefault(c => c.Id == item.Id);
                        if (!(answer != null && answer.IsExact == true && answer.IsFirstSubmit))
                        {
                            item.IsFirstSubmit = false;
                        }
                    }
                }
            }
            configAnswer = dataAnswer;
            return (number, isAnswerMissing, _linQAnswerHelper.IsAnswerHaveData(dataAnswer.Answers, nameof(DragAndDropSentenceOrderAnswers.Answer)));
        }

        private (int, bool, bool) HandleAnswer(ref object? configAnswer, DragAndDropListSentenceOrderAnswer? dataOldAnswer, DragAndDropListSentenceOrderQuestion? dataQuestion, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<DragAndDropListSentenceOrderAnswer>();
            int number = 0;
            bool isAnswerMissing = IsAnswerMissing(dataAnswer?.Answers, dataQuestion?.Contents, EnumQuestionType.DragAndDropListSentenceOrder, isSubmit, isMandatoryAnswer);
            if (((dataAnswer == null || dataAnswer.Answers == null) || !dataAnswer.Answers.Any()) || dataQuestion?.Contents == null || (isMandatoryAnswer && isAnswerMissing))
            {
                configAnswer = dataAnswer;
                return (default, isAnswerMissing, false);
            }
            if (!isMandatoryAnswer || (isMandatoryAnswer && !isAnswerMissing))
            {
                foreach (var item in dataAnswer.Answers)
                {
                    var indexAnswer = dataAnswer.Answers.IndexOf(item);
                    var question = dataQuestion.Contents.FirstOrDefault(c => c.Id == item.Id);
                    if (question != null)
                    {
                        var indexQuestion = dataQuestion.Contents.IndexOf(question);
                        item.IsExact = indexAnswer == indexQuestion;
                    }

                    if (isTryAgain && dataOldAnswer != null && dataOldAnswer.Answers != null)
                    {
                        var answer = dataOldAnswer.Answers.FirstOrDefault(c => c.Id == item.Id);
                        if (!(answer != null && answer.IsExact == true && answer.IsFirstSubmit))
                        {
                            item.IsFirstSubmit = false;
                        }
                    }
                }
                number = dataAnswer.Answers.All(x => x.IsExact == true) ? ++number : default;
            }
            configAnswer = dataAnswer;
            return (number, isAnswerMissing, _linQAnswerHelper.IsAnswerHaveData(dataAnswer.Answers));
        }

        private (int, bool, bool) HandleMultipleOptionAnswer(ref object? configAnswer, MultipleOptionSentenceCompletionAnswer? dataOldAnswer, MultipleOptionSentenceCompletionQuestion? dataQuestion, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<MultipleOptionSentenceCompletionAnswer>();
            int number = 0;
            bool isAnswerMissing = IsAnswerMissing(dataAnswer?.Answers, dataQuestion?.Contents, EnumQuestionType.MultipleOptionSentenceCompletion, isSubmit, isMandatoryAnswer);
            if (((dataAnswer == null || dataAnswer.Answers == null) || !dataAnswer.Answers.Any()) || dataQuestion?.Contents == null || (isMandatoryAnswer && isAnswerMissing))
            {
                configAnswer = dataAnswer;
                return (default, isAnswerMissing, false);
            }
            foreach (var item in dataAnswer.Answers)
            {
                var question = dataQuestion.Contents.FirstOrDefault(x => x.Id == item.Id);
                if (item.AnswerId.HasValue)
                {
                    if (question?.Answers != null && question.Answers.Count > 0 && question.Answers.Any(n => n.Id == item.AnswerId && n.IsCorrect == true))
                    {
                        number++;
                        item.IsExact = true;
                    }
                    else
                    {
                        item.IsExact = false;
                    }
                }
                else
                {
                    item.IsExact = default;
                }
                if (isTryAgain && dataOldAnswer != null && dataOldAnswer.Answers != null)
                {
                    var answer = dataOldAnswer.Answers.FirstOrDefault(x => x.Id == item.Id);
                    if (!(answer != null && answer.IsExact == true && answer.IsFirstSubmit))
                    {
                        item.IsFirstSubmit = false;
                    }
                }
            }
            configAnswer = dataAnswer;
            return (number, isAnswerMissing, _linQAnswerHelper.IsAnswerHaveData(dataAnswer.Answers, nameof(MultipleOptionSentenceCompletionAnswers.AnswerId)));
        }

        #region V1

        private (int, bool, bool) HandleAnswerMultipleChoiceV1(ref object? configAnswer, MultipleChoiceAnswerV1? dataOldAnswer, MultipleChoiceQuestionV1? dataQuestion, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<MultipleChoiceAnswerV1>();
            int number = 0;
            bool isAnswerMissing = IsAnswerMissing(dataAnswer?.Answers, dataQuestion?.Answers, EnumQuestionType.MultichoiceV1, isSubmit, isMandatoryAnswer);
            if (((dataAnswer == null || dataAnswer.Answers == null) || !dataAnswer.Answers.Any()) || (isMandatoryAnswer && isAnswerMissing))
            {
                configAnswer = dataAnswer;
                return (default, isAnswerMissing, false);
            }
            if (dataAnswer.Answers.GroupBy(x => x.Id).Any(x => x.Count() > 1))
            {
                configAnswer = dataAnswer;
                return (default, isAnswerMissing, false);
            }
            foreach (var item in dataAnswer.Answers)
            {
                var question = dataQuestion?.Answers.FirstOrDefault(x => x.Id == item.Id);
                if (!string.IsNullOrEmpty(item.Key) && question != null)
                {
                    var answerQuestion = question.Answers.FirstOrDefault(x => x.Key?.Trim().ToLower() == item.Key?.Trim().ToLower());
                    if (answerQuestion != null && answerQuestion.IsCorrect.HasValue && answerQuestion.IsCorrect.Value)
                    {
                        number++;
                        item.IsExact = true;
                    }
                    else
                    {
                        item.IsExact = false;
                    }
                }
                else
                {
                    item.IsExact = default;
                }
                if (isTryAgain && dataOldAnswer != null && dataOldAnswer.Answers != null)
                {
                    var answer = dataOldAnswer.Answers.FirstOrDefault(x => x.Id == item.Id);
                    if (!(answer != null && answer.IsExact == true && answer.IsFirstSubmit))
                    {
                        item.IsFirstSubmit = false;
                    }
                }
            }
            configAnswer = dataAnswer;
            return (number, isAnswerMissing, _linQAnswerHelper.IsAnswerHaveData(dataAnswer.Answers, nameof(ConfigAnswerV1.Key)));
        }

        private (int, bool, bool) HandleAnswerCheckListV1(ref object? configAnswer, MultipleChoiceAnswerV1? dataOldAnswer, CheckListQuestionV1? dataQuestion, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<MultipleChoiceAnswerV1>();
            int number = 0;
            bool isAnswerMissing = IsAnswerMissing(dataAnswer?.Answers, dataQuestion?.Answers, EnumQuestionType.CheckListV1, isSubmit, isMandatoryAnswer);
            if (((dataAnswer == null || dataAnswer.Answers == null) || !dataAnswer.Answers.Any()) || (isMandatoryAnswer && isAnswerMissing))
            {
                configAnswer = dataAnswer;
                return (default, isAnswerMissing, false);
            }
            if (dataAnswer.Answers.Count > dataQuestion?.Answers.Count(x => x.IsCorrect.HasValue && x.IsCorrect.Value))
            {
                configAnswer = dataAnswer;
                return (default, isAnswerMissing, false);
            }
            foreach (var item in dataAnswer.Answers)
            {
                var answerQuestion = dataQuestion?.Answers.FirstOrDefault(x => x.Id == item.Id);
                if (answerQuestion != null)
                {
                    if (answerQuestion.IsCorrect.HasValue && answerQuestion.IsCorrect.Value)
                    {
                        number++;
                        item.IsExact = true;
                    }
                    else
                    {
                        item.IsExact = false;
                    }
                }
                else
                {
                    item.IsExact = default;
                }

                if (isTryAgain && dataOldAnswer != null && dataOldAnswer.Answers != null)
                {
                    var answer = dataOldAnswer.Answers.FirstOrDefault(x => x.Id == item.Id);
                    if (!(answer != null && answer.IsExact == true && answer.IsFirstSubmit))
                    {
                        item.IsFirstSubmit = false;
                    }
                }
            }
            configAnswer = dataAnswer;
            return (number, isAnswerMissing, _linQAnswerHelper.IsAnswerHaveData(dataAnswer.Answers, nameof(ConfigAnswerV1.Key)));
        }

        private (int, bool, bool) HandleAnswerSummaryCompletionGapFill(ref object? configAnswer, MultipleChoiceAnswerV1? dataOldAnswer, CheckListQuestionV1? dataQuestion, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<MultipleChoiceAnswerV1>();
            int number = 0;
            bool isAnswerMissing = IsAnswerMissing(dataAnswer?.Answers, dataQuestion?.Answers, EnumQuestionType.SummaryCompletionGapFill, isSubmit, isMandatoryAnswer);
            if (((dataAnswer == null || dataAnswer.Answers == null) || !dataAnswer.Answers.Any()) || (isMandatoryAnswer && isAnswerMissing))
            {
                configAnswer = dataAnswer;
                return (default, isAnswerMissing, false);
            }
            foreach (var item in dataAnswer.Answers)
            {
                var answerQuestion = dataQuestion?.Answers.FirstOrDefault(x => x.Id == item.Id);
                if (answerQuestion != null)
                {
                    if (_linQAnswerHelper.CheckAnswer(answerQuestion.Content, item.Content))
                    {
                        number++;
                        item.IsExact = true;
                    }
                    else
                    {
                        item.IsExact = false;
                    }
                }
                else
                {
                    item.IsExact = default;
                }
                if (isTryAgain && dataOldAnswer != null && dataOldAnswer.Answers != null)
                {
                    var answer = dataOldAnswer.Answers.FirstOrDefault(x => x.Id == item.Id);
                    if (!(answer != null && answer.IsExact == true && answer.IsFirstSubmit))
                    {
                        item.IsFirstSubmit = false;
                    }
                }
            }
            configAnswer = dataAnswer;
            return (number, isAnswerMissing, _linQAnswerHelper.IsAnswerHaveData(dataAnswer.Answers, nameof(ConfigAnswerV1.Content)));
        }

        private (int, bool, bool) HandleAnswerFlowChartCompletion(ref object? configAnswer, MultipleChoiceAnswerV1? dataOldAnswer, FlowChartCompletionQuestion? dataQuestion, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<MultipleChoiceAnswerV1>();
            int number = 0;
            bool isAnswerMissing = IsAnswerMissing(dataAnswer?.Answers, dataQuestion?.Answers, EnumQuestionType.FlowChartCompletion, isSubmit, isMandatoryAnswer);
            if (((dataAnswer == null || dataAnswer.Answers == null) || !dataAnswer.Answers.Any()) || (isMandatoryAnswer && isAnswerMissing))
            {
                configAnswer = dataAnswer;
                return (default, isAnswerMissing, false);
            }
            foreach (var item in dataAnswer.Answers)
            {
                var answerQuestion = dataQuestion?.Answers.FirstOrDefault(x => x.Id == item.Id);
                if (answerQuestion != null)
                {
                    if (_linQAnswerHelper.CheckAnswer(answerQuestion.Content, item.Content))
                    {
                        number++;
                        item.IsExact = true;
                    }
                    else
                    {
                        item.IsExact = false;
                    }
                }
                else
                {
                    item.IsExact = default;
                }
                if (isTryAgain && dataOldAnswer != null && dataOldAnswer.Answers != null)
                {
                    var answer = dataOldAnswer.Answers.FirstOrDefault(x => x.Id == item.Id);
                    if (!(answer != null && answer.IsExact == true && answer.IsFirstSubmit))
                    {
                        item.IsFirstSubmit = false;
                    }
                }
            }
            configAnswer = dataAnswer;
            return (number, isAnswerMissing, _linQAnswerHelper.IsAnswerHaveData(dataAnswer.Answers, nameof(ConfigAnswerV1.Content)));
        }

        private (int, bool, bool) HandleAnswerTableCompletion(ref object? configAnswer, MultipleChoiceAnswerV1? dataOldAnswer, TableCompletionQuestion? dataQuestion, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<MultipleChoiceAnswerV1>();
            int number = 0;
            bool isAnswerMissing = IsAnswerMissing(dataAnswer?.Answers, dataQuestion?.AnswerTables, EnumQuestionType.TableCompletion, isSubmit, isMandatoryAnswer);
            if (((dataAnswer == null || dataAnswer.Answers == null) || !dataAnswer.Answers.Any()) || (isMandatoryAnswer && isAnswerMissing))
            {
                configAnswer = dataAnswer;
                return (default, isAnswerMissing, false);
            }
            foreach (var item in dataAnswer.Answers)
            {
                var answerQuestion = dataQuestion?.AnswerTables.FirstOrDefault(x => x.Id == item.Id);
                if (answerQuestion != null)
                {
                    if (_linQAnswerHelper.CheckAnswer(answerQuestion.Content, item.Content))
                    {
                        number++;
                        item.IsExact = true;
                    }
                    else
                    {
                        item.IsExact = false;
                    }
                }
                else
                {
                    item.IsExact = default;
                }
                if (isTryAgain && dataOldAnswer != null && dataOldAnswer.Answers != null)
                {
                    var answer = dataOldAnswer.Answers.FirstOrDefault(x => x.Id == item.Id);
                    if (!(answer != null && answer.IsExact == true && answer.IsFirstSubmit))
                    {
                        item.IsFirstSubmit = false;
                    }
                }
            }
            configAnswer = dataAnswer;
            return (number, isAnswerMissing, _linQAnswerHelper.IsAnswerHaveData(dataAnswer.Answers, nameof(ConfigAnswerV1.Content)));
        }

        private (int, bool, bool) HandleAnswerMatchingTask(ref object? configAnswer, MultipleChoiceAnswerV1? dataOldAnswer, MatchingTaskQuestion? dataQuestion, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<MultipleChoiceAnswerV1>();
            int number = 0;
            bool isAnswerMissing = IsAnswerMissing(dataAnswer?.Answers, dataQuestion?.Answers, EnumQuestionType.YesNoNotGivenDropDown, isSubmit, isMandatoryAnswer);
            if (((dataAnswer == null || dataAnswer.Answers == null) || !dataAnswer.Answers.Any()) || (isMandatoryAnswer && isAnswerMissing))
            {
                configAnswer = dataAnswer;
                return (default, isAnswerMissing, false);
            }
            foreach (var item in dataAnswer.Answers)
            {
                var answerQuestion = dataQuestion?.Answers.FirstOrDefault(x => x.Id == item.Id);
                if (answerQuestion != null)
                {
                    if (_linQAnswerHelper.CheckAnswer(answerQuestion.Key, item.Key))
                    {
                        number++;
                        item.IsExact = true;
                    }
                    else
                    {
                        item.IsExact = false;
                    }
                }
                else
                {
                    item.IsExact = default;
                }
                if (isTryAgain && dataOldAnswer != null && dataOldAnswer.Answers != null)
                {
                    var answer = dataOldAnswer.Answers.FirstOrDefault(x => x.Id == item.Id);
                    if (!(answer != null && answer.IsExact == true && answer.IsFirstSubmit))
                    {
                        item.IsFirstSubmit = false;
                    }
                }
            }
            configAnswer = dataAnswer;
            return (number, isAnswerMissing, _linQAnswerHelper.IsAnswerHaveData(dataAnswer.Answers, nameof(ConfigAnswerV1.Key)));
        }

        #endregion V1

        #endregion Handle Answer

        public object? GetConfigEmpty(EnumQuestionType type)
        {
            switch (type)
            {
                case EnumQuestionType.Multichoice:
                case EnumQuestionType.Dropdown:
                case EnumQuestionType.Checklist:
                    return new MultipleChoiceAnswer();

                case EnumQuestionType.Listing:
                    return new ListingAnswer();

                case EnumQuestionType.DragAndDropPicture:
                case EnumQuestionType.MatchingType1:
                case EnumQuestionType.MatchingType2:
                    return new MatchingTypeAnswer();

                case EnumQuestionType.GapFillScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByGap:
                case EnumQuestionType.GapFillScoreByGap:
                    return new GapFillAnswer();

                case EnumQuestionType.MultipleOptionSentenceCompletion:
                    return new MultipleOptionSentenceCompletionAnswer();

                case EnumQuestionType.ShortAnswerWordCount:
                    return new ShortAnswerWordCountBaseAnswer();

                case EnumQuestionType.ShortAnswerWordBase:
                    return new ShortAnswerWordBaseAnswer();

                case EnumQuestionType.DragAndDropSentenceOrder:
                case EnumQuestionType.DragAndDropListSentenceOrder:
                    return new DragAndDropSentenceOrderAnswer();

                case EnumQuestionType.ExercisePreparation:
                    return default;

                case EnumQuestionType.MultichoiceV1:
                case EnumQuestionType.CheckListV1:
                case EnumQuestionType.SummaryCompletionGapFill:
                case EnumQuestionType.CompletionDiagrams:
                case EnumQuestionType.YesNoNotGivenDropDown:
                case EnumQuestionType.TrueFalseNotGivenDropDown:
                case EnumQuestionType.MapLabelingDropDown:
                case EnumQuestionType.SummaryCompletionDropDown:
                case EnumQuestionType.MatchingParagraphInfo:
                case EnumQuestionType.MatchingHeading:
                case EnumQuestionType.FlowChartCompletion:
                case EnumQuestionType.TableCompletion:
                    return new MultipleChoiceAnswerV1();

                default:
                    return default;
            }
        }
    }
}
