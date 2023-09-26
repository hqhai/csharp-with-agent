// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers;
    using Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions;
    using Fsel.Course.Domain.Enums;

    public class AnswerTypeConverter
    {
        public (object?, int) GetTotalCorrectByAsnwerType(object? configAnswer, object? configQuestion, EnumQuestionType type)
        {
            int totalCorrect = default;
            switch (type)
            {
                case EnumQuestionType.Multichoice:
                case EnumQuestionType.Dropdown:
                case EnumQuestionType.Checklist:
                    totalCorrect = GetTotalCorrectTypeCheckListAnswer(ref configAnswer, configQuestion);
                    break;

                case EnumQuestionType.Listing:
                    totalCorrect = GetTotalCorrectTypeListingAnswer(ref configAnswer, configQuestion);
                    break;

                case EnumQuestionType.DragAndDropPicture:
                case EnumQuestionType.MatchingType1:
                case EnumQuestionType.MatchingType2:
                    totalCorrect = GetTotalCorrectTypeMaschingAnswer(ref configAnswer, configQuestion);
                    break;

                case EnumQuestionType.ShortAnswerWordBase:
                    totalCorrect = GetTotalCorrectTypeShortAnswerWordBase(ref configAnswer, configQuestion);
                    break;

                case EnumQuestionType.ShortAnswerWordCount:
                    totalCorrect = GetTotalCorrectTypeShortAnswerWordCount(ref configAnswer, configQuestion);
                    break;

                case EnumQuestionType.GapFillScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByQuestion:
                    totalCorrect = GetTotalCorrectTypeGapFillBySubAnswer(ref configAnswer, configQuestion);
                    break;

                case EnumQuestionType.GapFillWordBankScoreByGap:
                case EnumQuestionType.GapFillScoreByGap:
                    totalCorrect = GetTotalCorrectTypeGapFillGapAnswer(ref configAnswer, configQuestion);
                    break;

                case EnumQuestionType.DragAndDropSentenceOrder:
                    totalCorrect = GetTotalCorrectTypeDragDropOrderAnswer(ref configAnswer, configQuestion);
                    break;

                case EnumQuestionType.MultipleOptionSentenceCompletion:
                    totalCorrect = GetTotalCorrectTypeMultipleOptionAnswer(ref configAnswer, configQuestion);
                    break;

                case EnumQuestionType.ExercisePreparation:
                    break;

                default:
                    throw new ArgumentException("Invalid answer type");
            }

            return (configAnswer, totalCorrect);
        }

        private static int GetTotalCorrectTypeMultipleOptionAnswer(ref object? configAnswer, object? configQuestion)
        {
            var dataAnswer = configAnswer.Deserialize<MultipleOptionSentenceCompletionAnswer>();
            var dataQuestion = configQuestion.Deserialize<MultipleOptionSentenceCompletionQuestion>();
            int number = 0;
            if (dataAnswer != null && dataAnswer.Answers != null && dataQuestion != null && dataQuestion.Contents != null)
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
            configAnswer = dataAnswer;
            return number;
        }

        private static int GetTotalCorrectTypeMaschingAnswer(ref object? configAnswer, object? configQuestion)
        {
            var dataAnswer = configAnswer.Deserialize<MatchingTypeAnswer>();
            var dataQuestion = configQuestion.Deserialize<MatchingTypeQuestion>();
            int number = 0;
            if (dataAnswer != null && dataAnswer.Answers != null && dataQuestion != null && dataQuestion.From != null && dataQuestion.To != null && dataQuestion.Link != null)
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
            configAnswer = dataAnswer;
            return number;
        }

        private static int GetTotalCorrectTypeCheckListAnswer(ref object? configAnswer, object? configQuestion)
        {
            var dataAnswer = configAnswer.Deserialize<MutipleChoiceAnswer>();
            var dataQuestion = configQuestion.Deserialize<MutipleChoiceQuestion>();
            int number = 0;
            if (dataAnswer != null && dataAnswer.Answers != null && dataQuestion != null && dataQuestion.Contents != null)
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
            configAnswer = dataAnswer;
            return number;
        }

        private static int GetTotalCorrectTypeListingAnswer(ref object? configAnswer, object? configQuestion)
        {
            var dataAnswer = configAnswer.Deserialize<ListingAnswer>();
            var dataQuestion = configQuestion.Deserialize<ListingQuestion>();
            int number = 0;

            if (dataQuestion != null && dataAnswer != null && dataAnswer.Answers != null && dataAnswer.Answers.Count > 0 && dataAnswer.Answers.Count >= dataQuestion.ExactWordCount)
            {
                dataAnswer.IsExact = true;
                number++;
            }
            else if (dataAnswer != null)
            {
                dataAnswer.IsExact = false;
            }
            configAnswer = dataAnswer;
            return number;
        }

        private static int GetTotalCorrectTypeShortAnswerWordCount(ref object? configAnswer, object? configQuestion)
        {
            var dataAnswer = configAnswer.Deserialize<ShortAnswerWordCountBaseAnswer>();
            var dataQuestion = configQuestion.Deserialize<ShortAnswerQuestionWordCountBaseQuestion>();
            int number = 0;

            if (dataAnswer != null && dataAnswer.Answers != null && dataQuestion != null)
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
            }
            configAnswer = dataAnswer;
            return number;
        }

        private static bool IsShortAnswer(string question, string answer)
        {
            string q = " " + question.Trim().ToLower(CultureInfo.CurrentCulture).Replace('’', '\'').ToString() + " ";
            string a = " " + answer.Trim().ToLower(CultureInfo.CurrentCulture).Replace('’', '\'').ToString() + " ";
            return a.Contains(q, StringComparison.OrdinalIgnoreCase);
        }

        private static int GetTotalCorrectTypeShortAnswerWordBase(ref object? configAnswer, object? configQuestion)
        {
            var dataAnswer = configAnswer.Deserialize<ShortAnswerWordBaseAnswer>();
            var dataQuestion = configQuestion.Deserialize<ShortAnswerQuestionWordBaseQuestion>();
            int number = 0;

            if (dataAnswer != null && dataAnswer.Answers != null && dataQuestion != null && dataQuestion.Contents != null && dataQuestion.Contents.Count > 0)
            {
                if (dataQuestion.Contents.Any(p => IsShortAnswer(p, dataAnswer.Answers)))
                {
                    dataAnswer.IsExact = true;
                    number++;
                }
                else
                {
                    dataAnswer.IsExact = false;
                }
            }
            if (dataAnswer != null && dataAnswer.Answers != null && dataQuestion != null && dataQuestion.Content != null && dataQuestion.Content.Count > 0)
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
            }
            configAnswer = dataAnswer;
            return number;
        }

        private static int GetTotalCorrectTypeGapFillBySubAnswer(ref object? configAnswer, object? configQuestion)
        {
            var dataAnswer = configAnswer.Deserialize<GapFillAnswer>();
            var dataQuestion = configQuestion.Deserialize<GapFillQuestion>();
            int number = 0;
            if (dataAnswer != null && dataAnswer.Answers != null && dataAnswer.Answers.Count > 0 && dataQuestion != null && dataQuestion.Contents != null)
            {
                foreach (var item in dataAnswer.Answers)
                {
                    var question = dataQuestion.Contents.FirstOrDefault(c => c.Id == item.Id);
                    if (question != null && question.Words != null && question.Words.Count > 0 && item.Answer != null && item.Answer.Count > 0)
                    {
                        var isExacts = item.Answer.Select((word, index) => CheckAnswer(question.Words, word, index)).ToList();
                        item.IsExacts = isExacts;
                        if (isExacts.All(x => x))
                        {
                            number++;
                        }
                    }
                    else
                    {
                        item.IsExacts = new List<bool>();
                    }
                }
            }
            configAnswer = dataAnswer;
            return number;
        }

        private static bool CheckAnswer(IList<string> words, string word, int index)
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

        private static int GetTotalCorrectTypeGapFillGapAnswer(ref object? configAnswer, object? configQuestion)
        {
            var dataAnswer = configAnswer.Deserialize<GapFillAnswer>();
            var dataQuestion = configQuestion.Deserialize<GapFillQuestion>();
            int number = 0;
            if (dataAnswer != null && dataAnswer.Answers != null && dataQuestion != null && dataQuestion.Contents != null)
            {
                foreach (var item in dataAnswer.Answers)
                {
                    var question = dataQuestion.Contents.FirstOrDefault(c => c.Id == item.Id);
                    if (question != null && question.Words != null && question.Words.Count > 0 && item.Answer != null && item.Answer.Count > 0)
                    {
                        var isExacts = item.Answer.Select((word, index) => CheckAnswer(question.Words, word, index)).ToList();
                        item.IsExacts = isExacts;
                        number += isExacts.Count(x => x);
                    }
                    else
                    {
                        item.IsExacts = new List<bool>();
                    }
                }
            }
            configAnswer = dataAnswer;
            return number;
        }

        private static int GetTotalCorrectTypeDragDropOrderAnswer(ref object? configAnswer, object? configQuestion)
        {
            var dataAnswer = configAnswer.Deserialize<DragAndDropSentenceOrderAnswer>();
            var dataQuestion = configQuestion.Deserialize<DragAndDropSentenceOrderQuestion>();
            int number = 0;
            if (dataAnswer != null && dataAnswer.Answers != null && dataQuestion != null && dataQuestion.Contents != null)
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
            configAnswer = dataAnswer;
            return number;
        }
    }
}
