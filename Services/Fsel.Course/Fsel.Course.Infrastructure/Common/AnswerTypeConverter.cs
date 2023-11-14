// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using System;
    using System.Collections;
    using System.Linq;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers;
    using Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions;
    using Fsel.Course.Domain.Enums;

    public class AnswerTypeConverter
    {
        public (object?, int, bool) GetTotalCorrectByAnswerType(object? configAnswer, object? configQuestion, EnumQuestionType type, bool isSubmit = false, bool isMandatoryAnswer = false)
        {
            int totalCorrect = default;
            bool isAnswerMissing = default;
            switch (type)
            {
                case EnumQuestionType.Multichoice:
                case EnumQuestionType.Dropdown:
                case EnumQuestionType.Checklist:
                    (totalCorrect, isAnswerMissing) = GetTotalCorrectTypeCheckListAnswer(ref configAnswer, configQuestion, isSubmit, isMandatoryAnswer);
                    break;

                case EnumQuestionType.Listing:
                    (totalCorrect, isAnswerMissing) = GetTotalCorrectTypeListingAnswer(ref configAnswer, configQuestion, isSubmit, isMandatoryAnswer);
                    break;

                case EnumQuestionType.DragAndDropPicture:
                case EnumQuestionType.MatchingType1:
                case EnumQuestionType.MatchingType2:
                    (totalCorrect, isAnswerMissing) = GetTotalCorrectTypeMaschingAnswer(ref configAnswer, configQuestion, isSubmit, isMandatoryAnswer);
                    break;

                case EnumQuestionType.ShortAnswerWordBase:
                    (totalCorrect, isAnswerMissing) = GetTotalCorrectTypeShortAnswerWordBase(ref configAnswer, configQuestion, isSubmit, isMandatoryAnswer);
                    break;

                case EnumQuestionType.ShortAnswerWordCount:
                    (totalCorrect, isAnswerMissing) = GetTotalCorrectTypeShortAnswerWordCount(ref configAnswer, configQuestion, isSubmit, isMandatoryAnswer);
                    break;

                case EnumQuestionType.GapFillScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByQuestion:
                    (totalCorrect, isAnswerMissing) = GetTotalCorrectTypeGapFillBySubAnswer(ref configAnswer, configQuestion, isSubmit, isMandatoryAnswer);
                    break;

                case EnumQuestionType.GapFillWordBankScoreByGap:
                case EnumQuestionType.GapFillScoreByGap:
                    (totalCorrect, isAnswerMissing) = GetTotalCorrectTypeGapFillGapAnswer(ref configAnswer, configQuestion, isSubmit, isMandatoryAnswer);
                    break;

                case EnumQuestionType.DragAndDropSentenceOrder:
                    (totalCorrect, isAnswerMissing) = GetTotalCorrectTypeDragDropOrderAnswer(ref configAnswer, configQuestion, isSubmit, isMandatoryAnswer);
                    break;

                case EnumQuestionType.MultipleOptionSentenceCompletion:
                    (totalCorrect, isAnswerMissing) = GetTotalCorrectTypeMultipleOptionAnswer(ref configAnswer, configQuestion, isSubmit, isMandatoryAnswer);
                    break;

                case EnumQuestionType.ExercisePreparation:
                    break;

                default:
                    throw new ArgumentException("Invalid answer type");
            }

            return (configAnswer, totalCorrect, isAnswerMissing);
        }

        public object? AnswerTypeConverterObject(object? configAnswer, EnumQuestionType type, bool isDisableAnswers = false)
        {
            object? result;
            switch (type)
            {
                case EnumQuestionType.Multichoice:
                case EnumQuestionType.Dropdown:
                case EnumQuestionType.Checklist:
                    var multichoice = configAnswer.Deserialize<MultipleChoiceAnswer>();
                    result = isDisableAnswers ? ClearAnswers(multichoice) : multichoice;
                    break;

                case EnumQuestionType.Listing:
                    var listingQuestion = configAnswer.Deserialize<ListingAnswer>();
                    result = isDisableAnswers ? ClearAnswers(listingQuestion) : listingQuestion;
                    break;

                case EnumQuestionType.MatchingType1:
                case EnumQuestionType.MatchingType2:
                case EnumQuestionType.DragAndDropPicture:
                    var matchingTypeQuestion = configAnswer.Deserialize<MatchingTypeAnswer>();
                    result = isDisableAnswers ? ClearAnswers(matchingTypeQuestion) : matchingTypeQuestion;
                    break;

                case EnumQuestionType.ShortAnswerWordBase:
                    var shortAnswerQuestionWordBaseQuestion = configAnswer.Deserialize<ShortAnswerWordBaseAnswer>();
                    result = isDisableAnswers ? ClearAnswers(shortAnswerQuestionWordBaseQuestion) : shortAnswerQuestionWordBaseQuestion;
                    break;

                case EnumQuestionType.ShortAnswerWordCount:
                    var shortAnswerWordCount = configAnswer.Deserialize<ShortAnswerWordCountBaseAnswer>();
                    result = isDisableAnswers ? ClearAnswers(shortAnswerWordCount) : shortAnswerWordCount;
                    break;

                case EnumQuestionType.GapFillScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByGap:
                case EnumQuestionType.GapFillScoreByGap:
                    var gapFillQuestion = configAnswer.Deserialize<GapFillAnswer>();
                    result = isDisableAnswers ? ClearAnswers(gapFillQuestion) : gapFillQuestion;
                    break;

                case EnumQuestionType.DragAndDropSentenceOrder:
                    var dragAndDropSentenceOrderQuestion = configAnswer.Deserialize<DragAndDropSentenceOrderAnswer>();
                    result = isDisableAnswers ? ClearAnswers(dragAndDropSentenceOrderQuestion) : dragAndDropSentenceOrderQuestion;
                    break;

                case EnumQuestionType.MultipleOptionSentenceCompletion:
                    var multipleOption = configAnswer.Deserialize<MultipleOptionSentenceCompletionAnswer>();
                    result = isDisableAnswers ? ClearAnswers(multipleOption) : multipleOption;
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

        private static object? ClearAnswers(MultipleOptionSentenceCompletionAnswer? data)
        {
            if (data != null && data.Answers != null)
            {
                foreach (var item in data.Answers)
                {
                    item.IsExact = default;
                }
            }
            return data;
        }

        private static object? ClearAnswers(DragAndDropSentenceOrderAnswer? data)
        {
            if (data != null && data.Answers != null)
            {
                foreach (var item in data.Answers)
                {
                    item.IsExact = default;
                }
            }
            return data;
        }

        private static object? ClearAnswers(ShortAnswerWordBaseAnswer? data)
        {
            if (data != null && !string.IsNullOrEmpty(data.Answers))
            {
                data.IsExact = default;
            }
            return data;
        }

        private static object? ClearAnswers(ListingAnswer? data)
        {
            if (data != null && data.Answers != null)
            {
                data.IsExact = default;
            }
            return data;
        }

        private static object? ClearAnswers(MultipleChoiceAnswer? data)
        {
            if (data != null && data.Answers != null)
            {
                foreach (var item in data.Answers)
                {
                    item.IsExact = default;
                }
            }
            return data;
        }

        private static object? ClearAnswers(ShortAnswerWordCountBaseAnswer? data)
        {
            if (data != null)
            {
                data.IsExact = default;
            }
            return data;
        }

        private static object? ClearAnswers(MatchingTypeAnswer? data)
        {
            if (data != null && data.Answers != null)
            {
                foreach (var item in data.Answers)
                {
                    item.IsExact = default;
                }
            }
            return data;
        }

        private static object? ClearAnswers(GapFillAnswer? data)
        {
            if (data != null && data.Answers != null)
            {
                foreach (var item in data.Answers)
                {
                    item.IsExacts = item.IsExacts != null ? item.IsExacts.Select(x => x = default).ToList() : default;
                }
            }
            return data;
        }

        private static (int, bool) GetTotalCorrectTypeMultipleOptionAnswer(ref object? configAnswer, object? configQuestion, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<MultipleOptionSentenceCompletionAnswer>();
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
                        if (question?.Answers != null && question.Answers.Count > 0 && question.Answers.Any(n => (item.AnswerId.HasValue && n.Id == item.AnswerId) && n.IsCorrect == true))
                        {
                            number++;
                            item.IsExact = true;
                        }
                        else
                        {
                            item.IsExact = false;
                        }
                    }
                }
            }
            configAnswer = dataAnswer;
            return (number, isAnswerMissing);
        }

        private static (int, bool) GetTotalCorrectTypeMaschingAnswer(ref object? configAnswer, object? configQuestion, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<MatchingTypeAnswer>();
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
                    }
                }
            }
            configAnswer = dataAnswer;
            return (number, isAnswerMissing);
        }

        private static (int, bool) GetTotalCorrectTypeCheckListAnswer(ref object? configAnswer, object? configQuestion, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<MultipleChoiceAnswer>();
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
                        var isCheck = item.IsChecked && dataQuestion.Contents.Any(x => x.Id == item.Id && x.IsCorrect == item.IsChecked);
                        if (isCheck)
                        {
                            number++;
                        }
                        item.IsExact = isCheck;
                    }
                }
            }
            configAnswer = dataAnswer;
            return (number, isAnswerMissing);
        }

        private static (int, bool) GetTotalCorrectTypeListingAnswer(ref object? configAnswer, object? configQuestion, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<ListingAnswer>();
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
                    if (dataAnswer.Answers.Count >= dataQuestion.ExactWordCount)
                    {
                        dataAnswer.IsExact = true;
                        number++;
                    }
                }
            }
            configAnswer = dataAnswer;
            return (number, isAnswerMissing);
        }

        private static (int, bool) GetTotalCorrectTypeShortAnswerWordCount(ref object? configAnswer, object? configQuestion, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<ShortAnswerWordCountBaseAnswer>();
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
                }
            }
            configAnswer = dataAnswer;
            return (number, isAnswerMissing);
        }

        private static bool IsShortAnswer(string question, string answer)
        {
            string q = " " + question.Trim().ToLower().Replace('’', '\'').ToString() + " ";
            string a = " " + answer.Trim().ToLower().Replace('’', '\'').ToString() + " ";
            return a.Contains(q, StringComparison.OrdinalIgnoreCase);
        }

        private static (int, bool) GetTotalCorrectTypeShortAnswerWordBase(ref object? configAnswer, object? configQuestion, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<ShortAnswerWordBaseAnswer>();
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
                }
            }
            configAnswer = dataAnswer;
            return (number, isAnswerMissing);
        }

        private static (int, bool) GetTotalCorrectTypeGapFillBySubAnswer(ref object? configAnswer, object? configQuestion, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<GapFillAnswer>();
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
                        if (question != null && question.Words != null && question.Words.Count > 0 && item.Answer != null && item.Answer.Count > 0)
                        {
                            var isExacts = item.Answer.Select((word, index) => CheckAnswer(question.Words, word, index)).ToList();
                            item.IsExacts = isExacts;
                            if (isExacts.All(x => x == true))
                            {
                                number++;
                            }
                        }
                        else
                        {
                            item.IsExacts = new List<bool?>();
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
                    return objects.Any(x => (bool)x.GetPropValue(nameProperty));
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
                    if (word.Trim().ToLower().Replace('’', '\'') == item.Trim().ToLower().Replace('’', '\''))
                    {
                        return true;
                    }
                }
            }
            else if (words[index].Trim().ToLower().Replace('’', '\'') == word.Trim().ToLower().Replace('’', '\''))
            {
                return true;
            }

            return false;
        }

        private static (int, bool) GetTotalCorrectTypeGapFillGapAnswer(ref object? configAnswer, object? configQuestion, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<GapFillAnswer>();
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
                            var isExacts = item.Answer.Select((word, index) => CheckAnswer(question.Words, word, index)).ToList();
                            item.IsExacts = isExacts;
                            number += isExacts.Count(x => x == true);
                        }
                        else
                        {
                            item.IsExacts = new List<bool?>();
                        }
                    }
                }
            }
            configAnswer = dataAnswer;
            return (number, isAnswerMissing);
        }

        private static (int, bool) GetTotalCorrectTypeDragDropOrderAnswer(ref object? configAnswer, object? configQuestion, bool isSubmit, bool isMandatoryAnswer)
        {
            var dataAnswer = configAnswer.Deserialize<DragAndDropSentenceOrderAnswer>();
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
                    }
                }
            }
            configAnswer = dataAnswer;
            return (number, isAnswerMissing);
        }
    }
}
