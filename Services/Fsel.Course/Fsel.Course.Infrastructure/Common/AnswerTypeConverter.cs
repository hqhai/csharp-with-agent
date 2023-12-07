// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Linq;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers;
    using Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class AnswerTypeConverter
    {
        public (object?, int, bool) GetTotalCorrectByAnswerType(object? configAnswer, object? configOldAnswer, object? configQuestion, EnumQuestionType type, bool isTryAgain = false, bool isSubmit = false, bool isMandatoryAnswer = false)
        {
            int totalCorrect = default;
            bool isAnswerMissing = default;
            switch (type)
            {
                case EnumQuestionType.Multichoice:
                case EnumQuestionType.Dropdown:
                case EnumQuestionType.Checklist:
                    (totalCorrect, isAnswerMissing) = GetTotalCorrectTypeCheckListAnswer(ref configAnswer, configOldAnswer, configQuestion, isTryAgain, isSubmit, isMandatoryAnswer);
                    break;

                case EnumQuestionType.Listing:
                    (totalCorrect, isAnswerMissing) = GetTotalCorrectTypeListingAnswer(ref configAnswer, configOldAnswer, configQuestion, isTryAgain, isSubmit, isMandatoryAnswer);
                    break;

                case EnumQuestionType.DragAndDropPicture:
                case EnumQuestionType.MatchingType1:
                case EnumQuestionType.MatchingType2:
                    (totalCorrect, isAnswerMissing) = GetTotalCorrectTypeMaschingAnswer(ref configAnswer, configOldAnswer, configQuestion, isTryAgain, isSubmit, isMandatoryAnswer);
                    break;

                case EnumQuestionType.ShortAnswerWordBase:
                    (totalCorrect, isAnswerMissing) = GetTotalCorrectTypeShortAnswerWordBase(ref configAnswer, configOldAnswer, configQuestion, isTryAgain, isSubmit, isMandatoryAnswer);
                    break;

                case EnumQuestionType.ShortAnswerWordCount:
                    (totalCorrect, isAnswerMissing) = GetTotalCorrectTypeShortAnswerWordCount(ref configAnswer, configOldAnswer, configQuestion, isTryAgain, isSubmit, isMandatoryAnswer);
                    break;

                case EnumQuestionType.GapFillScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByQuestion:
                    (totalCorrect, isAnswerMissing) = GetTotalCorrectTypeGapFillBySubAnswer(ref configAnswer, configOldAnswer, configQuestion, isTryAgain, isSubmit, isMandatoryAnswer);
                    break;

                case EnumQuestionType.GapFillWordBankScoreByGap:
                case EnumQuestionType.GapFillScoreByGap:
                    (totalCorrect, isAnswerMissing) = GetTotalCorrectTypeGapFillGapAnswer(ref configAnswer, configOldAnswer, configQuestion, isTryAgain, isSubmit, isMandatoryAnswer);
                    break;

                case EnumQuestionType.DragAndDropSentenceOrder:
                    (totalCorrect, isAnswerMissing) = GetTotalCorrectTypeDragDropOrderAnswer(ref configAnswer, configOldAnswer, configQuestion, isTryAgain, isSubmit, isMandatoryAnswer);
                    break;

                case EnumQuestionType.MultipleOptionSentenceCompletion:
                    (totalCorrect, isAnswerMissing) = GetTotalCorrectTypeMultipleOptionAnswer(ref configAnswer, configOldAnswer, configQuestion, isTryAgain, isSubmit, isMandatoryAnswer);
                    break;

                case EnumQuestionType.ExercisePreparation:
                    break;

                default:
                    throw new ArgumentException("Invalid answer type");
            }

            return (configAnswer, totalCorrect, isAnswerMissing);
        }

        public object? AnswerTypeConverterObject(object? configAnswer, EnumQuestionType type, bool isShowSubStatus, EnumResultStatus status)
        {
            object? result;
            switch (type)
            {
                case EnumQuestionType.Multichoice:
                case EnumQuestionType.Dropdown:
                case EnumQuestionType.Checklist:
                    var multichoice = configAnswer.Deserialize<MultipleChoiceAnswer>();
                    result = GetAnswer(multichoice, isShowSubStatus, status);
                    break;

                case EnumQuestionType.Listing:
                    var listingQuestion = configAnswer.Deserialize<ListingAnswer>();
                    result = GetAnswer(listingQuestion, isShowSubStatus, status);
                    break;

                case EnumQuestionType.MatchingType1:
                case EnumQuestionType.MatchingType2:
                case EnumQuestionType.DragAndDropPicture:
                    var matchingTypeQuestion = configAnswer.Deserialize<MatchingTypeAnswer>();
                    result = GetAnswer(matchingTypeQuestion, isShowSubStatus, status);
                    break;

                case EnumQuestionType.ShortAnswerWordBase:
                    var shortAnswerQuestionWordBaseQuestion = configAnswer.Deserialize<ShortAnswerWordBaseAnswer>();
                    result = GetAnswer(shortAnswerQuestionWordBaseQuestion, isShowSubStatus, status);
                    break;

                case EnumQuestionType.ShortAnswerWordCount:
                    var shortAnswerWordCount = configAnswer.Deserialize<ShortAnswerWordCountBaseAnswer>();
                    result = GetAnswer(shortAnswerWordCount, isShowSubStatus, status);
                    break;

                case EnumQuestionType.GapFillScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByGap:
                case EnumQuestionType.GapFillScoreByGap:
                    var gapFillQuestion = configAnswer.Deserialize<GapFillAnswer>();
                    result = GetAnswer(gapFillQuestion, isShowSubStatus, status);
                    break;

                case EnumQuestionType.DragAndDropSentenceOrder:
                    var dragAndDropSentenceOrderQuestion = configAnswer.Deserialize<DragAndDropSentenceOrderAnswer>();
                    result = GetAnswer(dragAndDropSentenceOrderQuestion, isShowSubStatus, status);
                    break;

                case EnumQuestionType.MultipleOptionSentenceCompletion:
                    var multipleOption = configAnswer.Deserialize<MultipleOptionSentenceCompletionAnswer>();
                    result = GetAnswer(multipleOption, isShowSubStatus, status);
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

        private static bool? IsDisableAnswers(EnumResultStatus status, bool isFirstSubmit, bool? isExact, bool isShowSubStatus)
        {
            return ((status == EnumResultStatus.Process && isFirstSubmit && isExact == true) || isShowSubStatus) ? isExact : default;
        }

        private static object? GetAnswer(MultipleOptionSentenceCompletionAnswer? data, bool isShowSubStatus, EnumResultStatus status)
        {
            if (data != null && data.Answers != null && status != EnumResultStatus.Done)
            {
                foreach (var item in data.Answers)
                {
                    item.IsExact = IsDisableAnswers(status, item.IsFirstSubmit, item.IsExact, isShowSubStatus);
                }
            }
            return data;
        }

        private static object? GetAnswer(DragAndDropSentenceOrderAnswer? data, bool isShowSubStatus, EnumResultStatus status)
        {
            if (data != null && data.Answers != null && status != EnumResultStatus.Done)
            {
                foreach (var item in data.Answers)
                {
                    item.IsExact = IsDisableAnswers(status, item.IsFirstSubmit, item.IsExact, isShowSubStatus);
                }
            }
            return data;
        }

        private static object? GetAnswer(ShortAnswerWordBaseAnswer? data, bool isShowSubStatus, EnumResultStatus status)
        {
            if (data != null && status != EnumResultStatus.Done)
            {
                data.IsExact = IsDisableAnswers(status, data.IsFirstSubmit, data.IsExact, isShowSubStatus);
            }
            return data;
        }

        private static object? GetAnswer(ListingAnswer? data, bool isShowSubStatus, EnumResultStatus status)
        {
            if (data != null && status != EnumResultStatus.Done)
            {
                data.IsExact = IsDisableAnswers(status, data.IsFirstSubmit, data.IsExact, isShowSubStatus);
            }
            return data;
        }

        private static object? GetAnswer(MultipleChoiceAnswer? data, bool isShowSubStatus, EnumResultStatus status)
        {
            if (data != null && data.Answers != null && status != EnumResultStatus.Done)
            {
                foreach (var item in data.Answers)
                {
                    item.IsExact = IsDisableAnswers(status, item.IsFirstSubmit, item.IsExact, isShowSubStatus);
                }
            }
            return data;
        }

        private static object? GetAnswer(ShortAnswerWordCountBaseAnswer? data, bool isShowSubStatus, EnumResultStatus status)
        {
            if (data != null && status != EnumResultStatus.Done)
            {
                data.IsExact = IsDisableAnswers(status, data.IsFirstSubmit, data.IsExact, isShowSubStatus);
            }
            return data;
        }

        private static object? GetAnswer(MatchingTypeAnswer? data, bool isShowSubStatus, EnumResultStatus status)
        {
            if (data != null && data.Answers != null && status != EnumResultStatus.Done)
            {
                foreach (var item in data.Answers)
                {
                    item.IsExact = IsDisableAnswers(status, item.IsFirstSubmit, item.IsExact, isShowSubStatus);
                }
            }
            return data;
        }

        private static object? GetAnswer(GapFillAnswer? data, bool isShowSubStatus, EnumResultStatus status)
        {
            if (data != null && data.Answers != null && status != EnumResultStatus.Done)
            {
                foreach (var item in data.Answers)
                {
                    item.IsExacts = item.IsExacts?.Select((x, index) =>
                    {
                        return IsDisableAnswers(status, item.IsFirstSubmits != null && item.IsFirstSubmits[index], x, isShowSubStatus);
                    }).ToList();
                }
            }
            return data;
        }

        private static (int, bool) GetTotalCorrectTypeMultipleOptionAnswer(ref object? configAnswer, object? configOldAnswer, object? configQuestion, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<MultipleOptionSentenceCompletionAnswer>();
            var dataOldAnswer = configOldAnswer.Deserialize<MultipleOptionSentenceCompletionAnswer>();
            var dataQuestion = configQuestion.Deserialize<MultipleOptionSentenceCompletionQuestion>();
            int number = 0;
            bool isAnswerMissing = false;
            if (dataAnswer != null && dataQuestion != null && dataQuestion.Contents != null)
            {
                if (isMandatoryAnswer && isSubmit && IsNullOrEmptyDataHasValue(dataAnswer.Answers, "AnswerId"))
                {
                    isAnswerMissing = true;
                }
                if (dataAnswer.Answers != null && (!isMandatoryAnswer || (isMandatoryAnswer && !isAnswerMissing)))
                {
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
                }
            }
            configAnswer = dataAnswer;
            return (number, isAnswerMissing);
        }

        private static (int, bool) GetTotalCorrectTypeMaschingAnswer(ref object? configAnswer, object? configOldAnswer, object? configQuestion, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<MatchingTypeAnswer>();
            var dataOldAnswer = configOldAnswer.Deserialize<MatchingTypeAnswer>();
            var dataQuestion = configQuestion.Deserialize<MatchingTypeQuestion>();
            int number = default;
            bool isAnswerMissing = default;
            if (dataAnswer != null && dataQuestion != null && dataQuestion.From != null && dataQuestion.To != null && dataQuestion.Link != null)
            {
                if (isMandatoryAnswer && isSubmit && (!CheckAnswerCount(dataAnswer.Answers, dataQuestion.Link) || IsNullOrEmptyDataHasValue(dataAnswer.Answers, "ToId")))
                {
                    isAnswerMissing = true;
                }
                if (dataAnswer.Answers != null && (!isMandatoryAnswer || (isMandatoryAnswer && !isAnswerMissing)))
                {
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
                }
            }
            configAnswer = dataAnswer;
            return (number, isAnswerMissing);
        }

        private static (int, bool) GetTotalCorrectTypeCheckListAnswer(ref object? configAnswer, object? configOldAnswer, object? configQuestion, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<MultipleChoiceAnswer>();
            var dataOldAnswer = configOldAnswer.Deserialize<MultipleChoiceAnswer>();
            var dataQuestion = configQuestion.Deserialize<MutipleChoiceQuestion>();
            int number = 0;
            bool isAnswerMissing = default;
            if (dataAnswer != null && dataQuestion != null && dataQuestion.Contents != null)
            {
                if (isMandatoryAnswer && isSubmit && (!CheckAnswerCount(dataAnswer.Answers, dataQuestion.Contents) || !IsNullOrEmptyDataValueBool(dataAnswer.Answers, "IsChecked")))
                {
                    isAnswerMissing = true;
                }
                if (dataAnswer.Answers != null && (!isMandatoryAnswer || (isMandatoryAnswer && !isAnswerMissing)))
                {
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
                }
            }
            configAnswer = dataAnswer;
            return (number < 0 ? default : number, isAnswerMissing);
        }

        private static (int, bool) GetTotalCorrectTypeListingAnswer(ref object? configAnswer, object? configOldAnswer, object? configQuestion, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<ListingAnswer>();
            var dataOldAnswer = configOldAnswer.Deserialize<ListingAnswer>();
            var dataQuestion = configQuestion.Deserialize<ListingQuestion>();
            int number = 0;
            bool isAnswerMissing = default;
            if (dataQuestion != null && dataAnswer != null)
            {
                if (isMandatoryAnswer && IsNullOrEmptyData(dataAnswer.Answers, "Answers") && isSubmit)
                {
                    isAnswerMissing = true;
                }
                if (dataAnswer.Answers != null && (!isMandatoryAnswer || (isMandatoryAnswer && !isAnswerMissing)))
                {
                    if (dataAnswer.Answers.Where(x => !string.IsNullOrEmpty(x)).Count() >= dataQuestion.ExactWordCount)
                    {
                        dataAnswer.IsExact = true;
                        number++;
                    }
                    else
                    {
                        dataAnswer.IsExact = false;
                    }
                    if (isTryAgain && dataOldAnswer != null && dataOldAnswer.Answers != null)
                    {
                        if (!(dataOldAnswer.IsExact == true && dataOldAnswer.IsFirstSubmit))
                        {
                            dataOldAnswer.IsFirstSubmit = false;
                        }
                    }
                }
            }
            configAnswer = dataAnswer;
            return (number, isAnswerMissing);
        }

        private static (int, bool) GetTotalCorrectTypeShortAnswerWordCount(ref object? configAnswer, object? configOldAnswer, object? configQuestion, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<ShortAnswerWordCountBaseAnswer>();
            var dataOldAnswer = configOldAnswer.Deserialize<ShortAnswerWordCountBaseAnswer>();
            var dataQuestion = configQuestion.Deserialize<ShortAnswerQuestionWordCountBaseQuestion>();
            int number = 0;
            bool isAnswerMissing = default;
            if (dataAnswer != null && dataQuestion != null)
            {
                if (isMandatoryAnswer && string.IsNullOrEmpty(dataAnswer.Answers) && isSubmit)
                {
                    isAnswerMissing = true;
                }
                if (dataAnswer.Answers != null && (!isMandatoryAnswer || (isMandatoryAnswer && !isAnswerMissing)))
                {
                    var answerStrs = dataAnswer.Answers.Trim().Split(' ');
                    if (answerStrs != null && answerStrs.Length >= dataQuestion.ExactWordCount)
                    {
                        dataAnswer.IsExact = true;
                        number++;
                    }
                    else
                    {
                        dataAnswer.IsExact = false;
                    }
                    if (isTryAgain && dataOldAnswer != null && dataOldAnswer.Answers != null)
                    {
                        if (!(dataOldAnswer.IsExact == true && dataOldAnswer.IsFirstSubmit))
                        {
                            dataAnswer.IsFirstSubmit = false;
                        }
                    }
                }
            }
            configAnswer = dataAnswer;
            return (number, isAnswerMissing);
        }

        private static bool IsShortAnswer(string question, string answer)
        {
            string q = " " + question.Trim().ToLower(CultureInfo.CurrentCulture).Replace('’', '\'').ToString() + " ";
            string a = " " + answer.Trim().ToLower(CultureInfo.CurrentCulture).Replace('’', '\'').ToString() + " ";
            return a.Contains(q, StringComparison.OrdinalIgnoreCase);
        }

        private static (int, bool) GetTotalCorrectTypeShortAnswerWordBase(ref object? configAnswer, object? configOldAnswer, object? configQuestion, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<ShortAnswerWordBaseAnswer>();
            var dataOldAnswer = configOldAnswer.Deserialize<ShortAnswerWordBaseAnswer>();
            var dataQuestion = configQuestion.Deserialize<ShortAnswerQuestionWordBaseQuestion>();
            int number = 0;
            bool isAnswerMissing = default;
            if (dataAnswer != null && dataQuestion != null && dataQuestion.Content != null && dataQuestion.Content.Any())
            {
                if (isMandatoryAnswer && string.IsNullOrEmpty(dataAnswer.Answers) && isSubmit)
                {
                    isAnswerMissing = true;
                }
                if (dataAnswer.Answers != null && (!isMandatoryAnswer || (isMandatoryAnswer && !isAnswerMissing)))
                {
                    if (dataQuestion.Content.Any(p => IsShortAnswer(p, dataAnswer.Answers)))
                    {
                        dataAnswer.IsExact = true;
                        number++;
                    }
                    else
                    {
                        dataAnswer.IsExact = false;
                    }
                    if (isTryAgain && dataOldAnswer != null && dataOldAnswer.Answers != null)
                    {
                        if (!(dataOldAnswer.IsExact == true && dataOldAnswer.IsFirstSubmit))
                        {
                            dataAnswer.IsFirstSubmit = false;
                        }
                    }
                }
            }
            configAnswer = dataAnswer;
            return (number, isAnswerMissing);
        }

        private static (int, bool) GetTotalCorrectTypeGapFillBySubAnswer(ref object? configAnswer, object? configOldAnswer, object? configQuestion, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<GapFillAnswer>();
            var dataOldAnswer = configOldAnswer.Deserialize<GapFillAnswer>();
            var dataQuestion = configQuestion.Deserialize<GapFillQuestion>();
            int number = 0;
            bool isAnswerMissing = default;
            if (dataAnswer != null && dataQuestion != null && dataQuestion.Contents != null)
            {
                if (isMandatoryAnswer && isSubmit && (!CheckAnswerCount(dataAnswer.Answers, dataQuestion.Contents) || IsNullOrEmptyData(dataAnswer.Answers, "Answer")))
                {
                    isAnswerMissing = true;
                }
                if (dataAnswer.Answers != null && (!isMandatoryAnswer || (isMandatoryAnswer && !isAnswerMissing)))
                {
                    foreach (var item in dataAnswer.Answers)
                    {
                        var question = dataQuestion.Contents.FirstOrDefault(c => c.Id == item.Id);

                        if (question != null && question.Words?.Any() == true && item.Answer?.Any() == true)
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
                }
            }
            configAnswer = dataAnswer;
            return (number, isAnswerMissing);
        }

        private static bool IsNullOrEmptyData(object? data, string? nameProperty)
        {
            if (data is IList list && !string.IsNullOrEmpty(nameProperty))
            {
                var objects = list.Cast<object>().ToList();
                if (objects != null && objects.Any())
                {
                    return objects.Any(x =>
                     {
                         var datas = x.GetPropValue(nameProperty) as IList;
                         if (datas != null)
                         {
                             var listObject = datas?.Cast<object>().ToList();
                             return ((listObject == null || !listObject.Any()) || listObject.Any(x => string.IsNullOrEmpty(x.ToString())));
                         }
                         return string.IsNullOrEmpty(x.ToString());
                     });
                }
            }
            else
            {
                return string.IsNullOrEmpty(data?.ToString());
            }
            return false;
        }

        public static bool CheckAnswerCount(object? answer, object? question)
        {
            if (answer is IList listAnswer && question is IList listQuestion)
            {
                return listAnswer.Count == listQuestion.Count;
            }
            return true;
        }

        private static bool IsNullOrEmptyDataHasValue(object? data, string? nameProperty)
        {
            if (data is IList list && !string.IsNullOrEmpty(nameProperty))
            {
                var objects = list.Cast<object>().ToList();
                if (objects != null && objects.Any())
                {
                    return objects.Any(x =>
                    {
                        if (x.GetPropValue(nameProperty) is long number)
                        {
                            return number == 0;
                        }
                        var propertyValue = x.GetPropValue(nameProperty)?.ToString();
                        return string.IsNullOrEmpty(propertyValue);
                    });
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return string.IsNullOrEmpty(data?.ToString());
            }
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
            else
            {
                return string.IsNullOrEmpty(data?.ToString());
            }
            return false;
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

        private static (int, bool) GetTotalCorrectTypeGapFillGapAnswer(ref object? configAnswer, object? configOldAnswer, object? configQuestion, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<GapFillAnswer>();
            var dataOldAnswer = configOldAnswer.Deserialize<GapFillAnswer>();
            var dataQuestion = configQuestion.Deserialize<GapFillQuestion>();
            int number = 0;
            bool isAnswerMissing = default;
            if (dataAnswer != null && dataQuestion != null && dataQuestion.Contents != null)
            {
                if (isMandatoryAnswer && isSubmit && (!CheckAnswerCount(dataAnswer.Answers, dataQuestion.Contents) || IsNullOrEmptyData(dataAnswer.Answers, "Answer")))
                {
                    isAnswerMissing = true;
                }
                if (dataAnswer.Answers != null && (!isMandatoryAnswer || (isMandatoryAnswer && !isAnswerMissing)))
                {
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
                }
            }
            configAnswer = dataAnswer;
            return (number, isAnswerMissing);
        }

        private static (int, bool) GetTotalCorrectTypeDragDropOrderAnswer(ref object? configAnswer, object? configOldAnswer, object? configQuestion, bool isTryAgain, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<DragAndDropSentenceOrderAnswer>();
            var dataOldAnswer = configOldAnswer.Deserialize<DragAndDropSentenceOrderAnswer>();
            var dataQuestion = configQuestion.Deserialize<DragAndDropSentenceOrderQuestion>();
            int number = 0;
            bool isAnswerMissing = default;
            if (dataAnswer != null && dataQuestion != null && dataQuestion.Contents != null)
            {
                if (isMandatoryAnswer && isSubmit && (!CheckAnswerCount(dataAnswer.Answers, dataQuestion.Contents) || IsNullOrEmptyData(dataAnswer.Answers, "Answer")))
                {
                    isAnswerMissing = true;
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
            }
            configAnswer = dataAnswer;
            return (number, isAnswerMissing);
        }
    }
}
