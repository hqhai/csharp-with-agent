// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers;
    using Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions;
    using Fsel.Course.Domain.Enums;

    public class AnswerTypeCountConverter
    {
        public int GetTotalCorrectByAsnwerType(ref object? configAnswer, object? configQuestion, EnumQuestionType type)
        {
            switch (type)
            {
                case EnumQuestionType.Multichoice:
                case EnumQuestionType.Dropdown:
                case EnumQuestionType.Checklist:
                    return GetTotalCorrectTypeCheckListAnswer(ref configAnswer, configQuestion);

                case EnumQuestionType.Listing:
                    return GetTotalCorrectTypeListingAnswer(ref configAnswer, configQuestion);

                case EnumQuestionType.MatchingType1:
                case EnumQuestionType.MatchingType2:
                    return GetTotalCorrectTypeMaschingAnswer(ref configAnswer, configQuestion);

                case EnumQuestionType.ShortAnswerWordBase:
                    return GetTotalCorrectTypeShortAnswerWordBase(ref configAnswer, configQuestion);

                case EnumQuestionType.ShortAnswerWordCount:
                    return GetTotalCorrectTypeShortAnswerWordCount(ref configAnswer, configQuestion);

                case EnumQuestionType.GapFillScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByQuestion:
                    return GetTotalCorrectTypeGapFillBySubAnswer(ref configAnswer, configQuestion);

                case EnumQuestionType.GapFillWordBankScoreByGap:
                case EnumQuestionType.GapFillScoreByGap:
                    return GetTotalCorrectTypeGapFillGapAnswer(ref configAnswer, configQuestion);

                case EnumQuestionType.DragAndDropSentenceOrder:
                    return GetTotalCorrectTypeDragDropOrderAnswer(ref configAnswer, configQuestion);

                case EnumQuestionType.DragAndDropPicture:
                    return GetTotalCorrectTypeDragDropPictureAnswer(ref configAnswer, configQuestion);

                case EnumQuestionType.MultipleOptionSentenceCompletion:
                    return GetTotalCorrectTypeMultipleOptionAnswer(ref configAnswer, configQuestion);

                case EnumQuestionType.ExercisePreparation:
                    return default;

                default:
                    throw new ArgumentException("Invalid question type");
            }
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
                    if (dataQuestion.Contents.All(x => x.Id == item.Id && x.Answers != null && x.Answers.All(n => n.Id == item.AnswerId && (n.IsCorrect ?? default))))
                    {
                        number++;
                        item.IsExact = true;
                    }
                }
                return number;
            }
            return default;
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
                    if (dataQuestion.Link.Any(x => x.FromId == item.FromId && x.ToId == item.ToId))
                    {
                        number++;
                        item.IsExact = true;
                    }
                }
                configAnswer = dataAnswer;
                return number;
            }
            return default;
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
                    if (item.IsChecked && dataQuestion.Contents.Any(x => x.Id == item.Id && x.IsCorrect == item.IsChecked))
                    {
                        number++;
                        item.IsExact = true;
                    }
                }
                return number;
            }
            return default;
        }

        private static int GetTotalCorrectTypeListingAnswer(ref object? configAnswer, object? configQuestion)
        {
            var dataAnswer = configAnswer.Deserialize<ListingAnswer>();
            var dataQuestion = configQuestion.Deserialize<ListingQuestion>();

            if (dataQuestion != null && dataAnswer != null && dataQuestion.ExactWordCount == dataAnswer.Answers?.Count)
            {
                dataAnswer.IsExact = true;
                return 1;
            }

            return default;
        }

        private static int GetTotalCorrectTypeShortAnswerWordCount(ref object? configAnswer, object? configQuestion)
        {
            var dataAnswer = configAnswer.Deserialize<ListingAnswer>();
            var dataQuestion = configQuestion.Deserialize<ListingQuestion>();
            int number = 0;
            if (dataAnswer != null && dataAnswer.Answers != null && dataQuestion != null)
            {
                var answerStrs = dataAnswer.Answers;
                var numberAnswer = answerStrs.Sum(x => x.Length);
                if (dataQuestion.ExactWordCount == numberAnswer)
                {
                    number++;
                    dataAnswer.IsExact = true;
                }
                dataAnswer.IsExact = false;
                return number;
            }
            return default;
        }

        private static int GetTotalCorrectTypeShortAnswerWordBase(ref object? configAnswer, object? configQuestion)
        {
            var dataAnswer = configAnswer.Deserialize<ListingAnswer>();
            var dataQuestion = configQuestion.Deserialize<ListingQuestion>();
            int number = 0;
            if (dataAnswer != null && dataAnswer.Answers != null && dataQuestion != null)
            {
                var answerStrs = dataAnswer.Answers;
                var numberAnswer = answerStrs.Sum(x => x.Length);
                if (dataQuestion.ExactWordCount == numberAnswer)
                {
                    number++;
                    dataAnswer.IsExact = true;
                }
                dataAnswer.IsExact = false;
                return number;
            }
            return default;
        }

        //Cách tính điển totalcourse ở Gap Fill (Score by sub question) 3 từ đều đúng điểm +1
        //Cách tính điển totalcourse ở Gap Fill (Score by gap) 3 từ nào đúng thì cộng thêm 1 điểm

        private static int GetTotalCorrectTypeGapFillGapAnswer(ref object? configAnswer, object? configQuestion)
        {
            var dataAnswer = configAnswer.Deserialize<GapFillAnswer>();
            var dataQuestion = configQuestion.Deserialize<GapFillQuestion>();
            int number = 0;
            if (dataAnswer != null && dataAnswer.Answers != null && dataQuestion != null && dataQuestion.Contents != null)
            {
                foreach (var item in dataAnswer.Answers)
                {
                    foreach (var item1 in dataQuestion.Contents)
                    {
                        if (item.Answer != null)
                        {
                            foreach (var item2 in item.Answer)
                            {
                                if (item1.Words != null)
                                {
                                    var isCheck = item1.Words.Any(x => x == item2);
                                    if (isCheck)
                                    {
                                        number++;
                                    }
                                    else
                                    {
                                    }
                                }
                            }
                        }
                    }
                }
                return number;
            }
            return default;
        }

        private static int GetTotalCorrectTypeGapFillBySubAnswer(ref object? configAnswer, object? configQuestion)
        {
            var dataAnswer = configAnswer.Deserialize<GapFillAnswer>();
            var dataQuestion = configQuestion.Deserialize<GapFillQuestion>();
            int number = 0;
            if (dataAnswer != null && dataAnswer.Answers != null && dataQuestion != null && dataQuestion.Contents != null)
            {
                foreach (var item in dataAnswer.Answers)
                {
                    foreach (var item1 in dataQuestion.Contents)
                    {
                        if (item.Answer != null)
                        {
                            foreach (var item2 in item.Answer)
                            {
                                if (item1.Words != null)
                                {
                                    var isCheck = item1.Words.Any(x => x == item2);
                                    if (isCheck)
                                    {
                                        number++;
                                    }
                                    else
                                    {
                                    }
                                }
                            }
                        }
                    }
                }
                return number;
            }
            return default;
        }

        private static int GetTotalCorrectTypeDragDropOrderAnswer(ref object? configAnswer, object? configQuestion)
        {
            //var data = config.Deserialize<DragAndDropSentenceOrderAnswer>();
            //int number = 0;
            //if (data != null && data.Contents != null)
            //{
            //    foreach (var item in data.Contents)
            //    {
            //        number++;
            //    }
            //    return number;
            //}
            var dataAnswer = configAnswer.Deserialize<ListingAnswer>();
            var dataQuestion = configQuestion.Deserialize<ListingQuestion>();
            int number = 0;
            if (dataAnswer != null && dataAnswer.Answers != null && dataQuestion != null)
            {
                var answerStrs = dataAnswer.Answers;
                var numberAnswer = answerStrs.Sum(x => x.Length);
                if (dataQuestion.ExactWordCount == numberAnswer)
                {
                    number++;
                    dataAnswer.IsExact = true;
                }
                dataAnswer.IsExact = false;
                return number;
            }
            return default;
        }

        private static int GetTotalCorrectTypeDragDropPictureAnswer(ref object? configAnswer, object? configQuestion)
        {
            //var data = config.Deserialize<DragAndDropPictureAnswer>();
            //int number = 0;
            //if (data != null && data.Contents != null)
            //{
            //    foreach (var item in data.Contents)
            //    {
            //        item.Images.ForEach(y =>
            //        {
            //            number++;
            //        });
            //    }
            //    return number;
            //}
            var dataAnswer = configAnswer.Deserialize<ListingAnswer>();
            var dataQuestion = configQuestion.Deserialize<ListingQuestion>();
            int number = 0;
            if (dataAnswer != null && dataAnswer.Answers != null && dataQuestion != null)
            {
                var answerStrs = dataAnswer.Answers;
                var numberAnswer = answerStrs.Sum(x => x.Length);
                if (dataQuestion.ExactWordCount == numberAnswer)
                {
                    number++;
                    dataAnswer.IsExact = true;
                }
                dataAnswer.IsExact = false;
                return number;
            }
            return default;
        }
    }
}
