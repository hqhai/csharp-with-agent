// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Linq;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers;
    using Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class AnswerTypeConverter
    {
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

                case EnumQuestionType.MultipleOptionSentenceCompletion:
                    (totalCorrect, isAnswerMissing, isAnswered) = HandleMultipleOptionAnswer(ref configAnswer, configOldAnswer.Deserialize<MultipleOptionSentenceCompletionAnswer>(), question.Config.Deserialize<MultipleOptionSentenceCompletionQuestion>(), isTryAgain, isSubmit, isMandatoryAnswer);
                    break;

                case EnumQuestionType.ExercisePreparation:
                    (totalCorrect, isAnswerMissing, isAnswered) = (default, false, true);
                    break;

                default:
                    return default;
            }
            return (configAnswer, totalCorrect, isAnswerMissing, isAnswered);
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

                case EnumQuestionType.MultipleOptionSentenceCompletion:
                    var multipleOption = configAnswer.Deserialize<MultipleOptionSentenceCompletionAnswer>();
                    result = GetAnswer(multipleOption, isShowSubStatus, status, isDisableAnswer);
                    break;

                case EnumQuestionType.ExercisePreparation:
                    var exercisePreparation = configAnswer.Deserialize<ExercisePreparationQuestion>();
                    result = exercisePreparation;
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

        private static bool IsAnswerMissing(object? dataAnswer, object? dataQuestion, EnumQuestionType type, bool isSubmit = false, bool isMandatoryAnswer = false)
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
                    return (!CheckAnswerCount(dataAnswer, dataQuestion) || !IsNullOrEmptyDataValueBool(dataAnswer, "IsChecked"));

                case EnumQuestionType.Listing:
                    return IsNullOrEmptyData(dataAnswer);

                case EnumQuestionType.DragAndDropPicture:
                case EnumQuestionType.MatchingType1:
                case EnumQuestionType.MatchingType2:
                    return (!CheckAnswerCount(dataAnswer, dataQuestion) || IsNullOrEmptyDataHasValue(dataAnswer, "ToId"));

                case EnumQuestionType.ShortAnswerWordBase:
                case EnumQuestionType.ShortAnswerWordCount:
                    return string.IsNullOrEmpty(dataAnswer?.ToString());

                case EnumQuestionType.GapFillScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByGap:
                case EnumQuestionType.GapFillScoreByGap:
                case EnumQuestionType.DragAndDropSentenceOrder:
                    return (!CheckAnswerCount(dataAnswer, dataQuestion) || IsNullOrEmptyData(dataAnswer, "Answer"));

                case EnumQuestionType.MultipleOptionSentenceCompletion:
                    return IsNullOrEmptyDataHasValue(dataAnswer, nameof(MultipleOptionSentenceCompletionAnswers.AnswerId));

                default:
                    return default;
            }
        }

        private static (int, bool, bool) HandleCheckListAnswer(ref object? configAnswer, MultipleChoiceAnswer? dataOldAnswer, MultipleChoiceQuestion? dataQuestion, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            int number = 0;
            var dataAnswer = configAnswer.Deserialize<MultipleChoiceAnswer>();
            bool isAnswerMissing = IsAnswerMissing(dataAnswer?.Answers, dataQuestion?.Contents, EnumQuestionType.Multichoice, isSubmit, isMandatoryAnswer);
            if (dataQuestion?.Contents == null || dataAnswer?.Answers == null || (isMandatoryAnswer && isAnswerMissing))
            {
                return (default, isAnswerMissing, IsNullOrEmptyDataValueBool(dataAnswer?.Answers, nameof(MultipleChoiceAnswers.IsChecked)));
            }
            foreach (var item in dataAnswer.Answers)
            {
                if (item.IsChecked)
                {
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
                else
                {
                    item.IsExact = default;
                }
            }
            configAnswer = dataAnswer;
            return (number < 0 ? default : number, isAnswerMissing, IsNullOrEmptyDataValueBool(dataAnswer?.Answers, nameof(MultipleChoiceAnswers.IsChecked)));
        }

        private static (int, bool, bool) HandleListingAnswer(ref object? configAnswer, ListingAnswer? dataOldAnswer, ListingQuestion? dataQuestion, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            int number = 0;
            var dataAnswer = configAnswer.Deserialize<ListingAnswer>();
            bool isAnswerMissing = IsAnswerMissing(dataAnswer?.Answers, default, EnumQuestionType.Listing, isSubmit, isMandatoryAnswer);
            if (dataAnswer?.Answers == null || (isMandatoryAnswer && isAnswerMissing))
            {
                return (default, isAnswerMissing, IsAnswerHaveData(dataAnswer?.Answers));
            }

            if (dataAnswer.Answers.Where(x => !string.IsNullOrEmpty(x)).Count() >= dataQuestion?.ExactWordCount)
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
            return (number, isAnswerMissing, IsAnswerHaveData(dataAnswer?.Answers));
        }

        private static (int, bool, bool) HandleMaschingTypeAnswer(ref object? configAnswer, MatchingTypeAnswer? dataOldAnswer, MatchingTypeQuestion? dataQuestion, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<MatchingTypeAnswer>();
            int number = default;
            bool isAnswerMissing = IsAnswerMissing(dataAnswer?.Answers, dataQuestion?.Link, EnumQuestionType.MatchingType1, isSubmit, isMandatoryAnswer);
            if (dataAnswer?.Answers == null || dataQuestion?.Link == null || (isMandatoryAnswer && isAnswerMissing))
            {
                return (default, isAnswerMissing, IsAnswerHaveData(dataAnswer?.Answers, nameof(MatchingTypeAnswers.ToId)));
            }
            foreach (var item in dataAnswer.Answers)
            {
                if (dataQuestion.Link.Any(x => x.FromId == item.FromId && (item.ToId.HasValue && x.ToId == item.ToId)))
                {
                    number++;
                    item.IsExact = true;
                }
                else
                {
                    item.IsExact = false;
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
            return (number, isAnswerMissing, IsAnswerHaveData(dataAnswer?.Answers, nameof(MatchingTypeAnswers.ToId)));
        }

        private static (int, bool, bool) HandleShortAnswerWordCount(ref object? configAnswer, ShortAnswerWordCountBaseAnswer? dataOldAnswer, ShortAnswerQuestionWordCountBaseQuestion? dataQuestion, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<ShortAnswerWordCountBaseAnswer>();
            int number = 0;
            bool isAnswerMissing = IsAnswerMissing(dataAnswer?.Answers, default, EnumQuestionType.ShortAnswerWordCount, isSubmit, isMandatoryAnswer);
            if (dataAnswer?.Answers == null || (isMandatoryAnswer && isAnswerMissing))
            {
                return (default, isAnswerMissing, string.IsNullOrEmpty(dataAnswer?.Answers));
            }
            var answerStrs = dataAnswer.Answers.Trim().Split(' ');
            if (answerStrs != null && answerStrs.Length >= dataQuestion?.ExactWordCount)
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
            return (number, isAnswerMissing, !string.IsNullOrEmpty(dataAnswer?.Answers));
        }

        private static (int, bool, bool) HandleShortAnswerWordBase(ref object? configAnswer, ShortAnswerWordBaseAnswer? dataOldAnswer, ShortAnswerQuestionWordBaseQuestion? dataQuestion, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<ShortAnswerWordBaseAnswer>();
            int number = 0;
            bool isAnswerMissing = IsAnswerMissing(dataAnswer?.Answers, dataQuestion?.Content, EnumQuestionType.ShortAnswerWordBase, isSubmit, isMandatoryAnswer);
            if (dataAnswer?.Answers == null || dataQuestion?.Content == null || (isMandatoryAnswer && isAnswerMissing))
            {
                return (default, isAnswerMissing, string.IsNullOrEmpty(dataAnswer?.Answers));
            }
            if (dataQuestion.Content.Any(p => IsShortAnswer(p, dataAnswer.Answers)))
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
            return (number, isAnswerMissing, !string.IsNullOrEmpty(dataAnswer?.Answers));
        }

        private static (int, bool, bool) HandleGapFillBySubAnswer(ref object? configAnswer, GapFillAnswer? dataOldAnswer, GapFillQuestion? dataQuestion, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<GapFillAnswer>();
            int number = 0;
            bool isAnswerMissing = IsAnswerMissing(dataAnswer?.Answers, dataQuestion?.Contents, EnumQuestionType.GapFillScoreByQuestion, isSubmit, isMandatoryAnswer);
            if (dataAnswer?.Answers == null || dataQuestion?.Contents == null || (isMandatoryAnswer && isAnswerMissing))
            {
                return (default, isAnswerMissing, IsAnswerHaveData(dataAnswer?.Answers, nameof(GapFillAnswers.Answer)));
            }
            foreach (var item in dataAnswer.Answers)
            {
                var question = dataQuestion.Contents.FirstOrDefault(c => c.Id == item.Id);
                if (question?.Words != null && item.Answer?.Any() == true)
                {
                    item.IsExacts = item.Answer.Select((word, index) => CheckAnswer(question.Words, word, index)).ToList();
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
                            foreach (var data in answer.IsExacts)
                            {
                                var index = answer.IsExacts.IndexOf(data);
                                if (answer.IsFirstSubmits[index] && data.HasValue && !data.Value)
                                {
                                    isFirstSubmit.Add(false);
                                }
                                else
                                {
                                    isFirstSubmit.Add(answer.IsFirstSubmits[index]);
                                }
                            }
                            item.IsFirstSubmits = isFirstSubmit;
                        }
                    }
                }
            }
            configAnswer = dataAnswer;
            return (number, isAnswerMissing, IsAnswerHaveData(dataAnswer?.Answers, nameof(GapFillAnswers.Answer)));
        }

        private static (int, bool, bool) HandleGapFillGapAnswer(ref object? configAnswer, GapFillAnswer? dataOldAnswer, GapFillQuestion? dataQuestion, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<GapFillAnswer>();
            int number = 0;
            bool isAnswerMissing = IsAnswerMissing(dataAnswer?.Answers, dataQuestion?.Contents, EnumQuestionType.GapFillScoreByQuestion, isSubmit, isMandatoryAnswer);
            if (dataAnswer?.Answers == null || dataQuestion?.Contents == null || (isMandatoryAnswer && isAnswerMissing))
            {
                return (default, isAnswerMissing, IsAnswerHaveData(dataAnswer?.Answers, nameof(GapFillAnswers.Answer)));
            }
            foreach (var item in dataAnswer.Answers)
            {
                var question = dataQuestion.Contents.FirstOrDefault(c => c.Id == item.Id);
                if (question != null && question.Words != null && question.Words.Any() && item.Answer != null && item.Answer.Any())
                {
                    item.IsExacts = item.Answer.Select((word, index) => CheckAnswer(question.Words, word, index)).ToList();
                    number += item.IsExacts.Count(x => x == true);
                    if (isTryAgain && dataOldAnswer != null && dataOldAnswer.Answers != null)
                    {
                        var answer = dataOldAnswer.Answers.FirstOrDefault(x => x.Id == item.Id);
                        if (answer != null && answer.IsFirstSubmits != null && answer.IsExacts != null)
                        {
                            var isFirstSubmit = new List<bool>();
                            foreach (var data in answer.IsExacts)
                            {
                                var index = answer.IsExacts.IndexOf(data);
                                if (answer.IsFirstSubmits[index] && data.HasValue && !data.Value)
                                {
                                    isFirstSubmit.Add(false);
                                }
                                else
                                {
                                    isFirstSubmit.Add(answer.IsFirstSubmits[index]);
                                }
                            }
                            item.IsFirstSubmits = isFirstSubmit;
                        }
                    }
                }
            }
            configAnswer = dataAnswer;
            return (number, isAnswerMissing, IsAnswerHaveData(dataAnswer?.Answers, nameof(GapFillAnswers.Answer)));
        }

        private static (int, bool, bool) HandleDragDropOrderAnswer(ref object? configAnswer, DragAndDropSentenceOrderAnswer? dataOldAnswer, DragAndDropSentenceOrderQuestion? dataQuestion, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<DragAndDropSentenceOrderAnswer>();
            int number = 0;
            bool isAnswerMissing = IsAnswerMissing(dataAnswer?.Answers, dataQuestion?.Contents, EnumQuestionType.DragAndDropSentenceOrder, isSubmit, isMandatoryAnswer);
            if (dataAnswer?.Answers == null || dataQuestion?.Contents == null || (isMandatoryAnswer && isAnswerMissing))
            {
                return (default, isAnswerMissing, IsAnswerHaveData(dataAnswer?.Answers, nameof(GapFillAnswers.Answer)));
            }
            if (dataAnswer.Answers != null && (!isMandatoryAnswer || (isMandatoryAnswer && !isAnswerMissing)))
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
            return (number, isAnswerMissing, IsAnswerHaveData(dataAnswer?.Answers, nameof(GapFillAnswers.Answer)));
        }

        private static (int, bool, bool) HandleMultipleOptionAnswer(ref object? configAnswer, MultipleOptionSentenceCompletionAnswer? dataOldAnswer, MultipleOptionSentenceCompletionQuestion? dataQuestion, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<MultipleOptionSentenceCompletionAnswer>();
            int number = 0;
            bool isAnswerMissing = IsAnswerMissing(dataAnswer?.Answers, dataQuestion?.Contents, EnumQuestionType.MultipleOptionSentenceCompletion, isSubmit, isMandatoryAnswer);
            if (dataAnswer?.Answers == null || dataQuestion?.Contents == null || (isMandatoryAnswer && isAnswerMissing))
            {
                return (default, isAnswerMissing, IsAnswerHaveData(dataAnswer?.Answers, nameof(GapFillAnswers.Answer)));
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
            return (number, isAnswerMissing, IsAnswerHaveData(dataAnswer?.Answers, nameof(MultipleOptionSentenceCompletionAnswers.AnswerId)));
        }

        private static bool IsShortAnswer(string? question, string? answer)
        {
            string q = " " + question?.Trim().ToLower(CultureInfo.CurrentCulture).Replace('’', '\'').ToString() + " ";
            string a = " " + answer?.Trim().ToLower(CultureInfo.CurrentCulture).Replace('’', '\'').ToString() + " ";
            return a.Contains(q, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsAnswerHaveData(object? data, string? nameProperty = default)
        {
            if (data is IList list)
            {
                var objects = list.Cast<object>().ToList();
                if (objects != null && objects.Any())
                {
                    return objects.Any(x => !string.IsNullOrEmpty(nameProperty) ? IsAnswerHaveData(x.GetPropValue(nameProperty)) : IsAnswerHaveData(x));
                }
                return false;
            }
            return !string.IsNullOrEmpty(data?.ToString());
        }

        private static bool IsNullOrEmptyData(object? data, string? nameProperty = default)
        {
            if (data is IList list)
            {
                var objects = list.Cast<object>().ToList();
                if (objects != null && objects.Any())
                {
                    return objects.Any(x => string.IsNullOrEmpty(nameProperty) ? IsNullOrEmptyData(x) : IsNullOrEmptyData(x.GetPropValue(nameProperty)));
                }
                return true;
            }
            return string.IsNullOrEmpty(data?.ToString());
        }

        private static bool IsNullOrEmptyDataHasValue(object? data, string? nameProperty)
        {
            if (data is IList list && !string.IsNullOrEmpty(nameProperty))
            {
                var objects = list.Cast<object>().ToList();
                return objects.Any(x => IsNullOrEmptyDataHasValue(x.GetPropValue(nameProperty), nameProperty));
            }
            return string.IsNullOrEmpty(data?.ToString());
        }

        public static bool CheckAnswerCount(object? answer, object? question)
        {
            if (answer is IList listAnswer && question is IList listQuestion)
            {
                return listAnswer.Count == listQuestion.Count;
            }
            return true;
        }

        private static bool IsNullOrEmptyDataValueBool(object? data, string? nameProperty)
        {
            if (data is IList list && !string.IsNullOrEmpty(nameProperty))
            {
                var objects = list.Cast<object>().ToList();
                if (objects != null && objects.Any())
                {
                    return objects.Any(x => x.GetPropValue<bool>(nameProperty));
                }
            }
            return string.IsNullOrEmpty(data?.ToString());
        }

        private static bool? CheckAnswer(IList<string> words, string word, int index)
        {
            if (words[index].IndexOf('|', StringComparison.Ordinal) != -1)
            {
                string[] questionWords = words[index].Split('|');
                foreach (var item in questionWords)
                {
                    if (word.Trim().ToLower(CultureInfo.CurrentCulture).Replace('’', '\'') == item.Trim().ToLower(CultureInfo.CurrentCulture).Replace('’', '\''))
                    {
                        return true;
                    }
                }
            }
            else if (words[index].Trim().ToLower(CultureInfo.CurrentCulture).Replace('’', '\'') == word.Trim().ToLower(CultureInfo.CurrentCulture).Replace('’', '\''))
            {
                return true;
            }

            return false;
        }

        internal (object answerConfig, int correctCount, bool isAnswerMissing, bool isAnswered) GetTotalCorrectByAnswerType(object answer, object oldAnswer, object config, EnumQuestionType questionType, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            throw new NotImplementedException();
        }
    }
}
