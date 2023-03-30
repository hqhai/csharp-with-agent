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
        public int GetTotalCorrectByAsnwerType(object configAnswer, object configQuestion, EnumQuestionType type)
        {
            switch (type)
            {
                case EnumQuestionType.Multichoice:
                case EnumQuestionType.Dropdown:
                case EnumQuestionType.Checklist:
                    return GetTotalCorrectTypeCheckListAnswer(configAnswer, configQuestion);

                case EnumQuestionType.Listing:
                    return GetTotalCorrectTypeListingAnswer(configAnswer, configQuestion);

                case EnumQuestionType.MatchingType1:
                case EnumQuestionType.MatchingType2:
                    return GetTotalCorrectTypeMaschingAnswer(configAnswer, configQuestion);

                case EnumQuestionType.ShortAnswerWordBase:
                    return GetTotalCorrectTypeShortAnswerWordBase(configAnswer, configQuestion);

                case EnumQuestionType.ShortAnswerWordCount:
                    return GetTotalCorrectTypeShortAnswerWordCount(configAnswer, configQuestion);

                case EnumQuestionType.GapFillScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByQuestion:
                    return GetTotalCorrectTypeGapFillBySubAnswer(configAnswer, configQuestion);

                case EnumQuestionType.GapFillWordBankScoreByGap:
                case EnumQuestionType.GapFillScoreByGap:
                    return GetTotalCorrectTypeGapFillGapAnswer(configAnswer, configQuestion);

                case EnumQuestionType.DragAndDropSentenceOrder:
                    return GetTotalCorrectTypeDragDropOrderAnswer(configAnswer, configQuestion);

                case EnumQuestionType.DragAndDropPicture:
                    return GetTotalCorrectTypeDragDropPictureAnswer(configAnswer, configQuestion);

                case EnumQuestionType.MultipleOptionSentenceCompletion:
                    return GetTotalCorrectTypeMultipleOptionAnswer(configAnswer, configQuestion);

                case EnumQuestionType.ExercisePreparation:
                    return default;

                default:
                    throw new ArgumentException("Invalid question type");
            }
        }

        private static int GetTotalCorrectTypeMultipleOptionAnswer(object configAnswer, object configQuestion)
        {
            var dataAnswer = configAnswer.Deserialize<MultipleOptionSentenceCompletionAnswer>();
            var dataQuestion = configQuestion.Deserialize<MultipleOptionSentenceCompletionQuestion>();
            int number = 0;
            if (dataAnswer != null && dataAnswer.Answers != null && dataQuestion != null && dataQuestion.Contents != null)
            {
                foreach (var item in dataQuestion.Contents)
                {
                    foreach (var item1 in dataAnswer.Answers)
                    {
                        var isCheck = item?.Answers?.Any(x => x.Id == item1.Id && x.IsCorrect == true);
                        if (isCheck == true)
                        {
                            number++;
                            item1.IsExact = true;
                        }
                        else
                        {
                            item1.IsExact = false;
                        }
                    }
                }
                return number;
            }
            return default;
        }

        private static int GetTotalCorrectTypeMaschingAnswer(object configAnswer, object configQuestion)
        {
            var dataAnswer = configAnswer.Deserialize<MatchingTypeAnswer>();
            var dataQuestion = configQuestion.Deserialize<MatchingTypeQuestion>();
            int number = 0;
            if (dataAnswer != null && dataAnswer.Answers != null && dataQuestion != null && dataQuestion.From != null && dataQuestion.Tos != null && dataQuestion.Links != null)
            {
                foreach (var item in dataAnswer.Answers)
                {
                    var isCheck = dataQuestion.Links.Any(x => x.FromId == item.FromId && x.Told == item.Told);
                    if (isCheck)
                    {
                        number++;
                        item.IsExact = true;
                    }
                    else
                    {
                        item.IsExact = false;
                    }
                }
                return number;
            }
            return default;
        }

        private static int GetTotalCorrectTypeCheckListAnswer(object configAnswer, object configQuestion)
        {
            var dataAnswer = configAnswer.Deserialize<MutipleChoiceAnswer>();
            var dataQuestion = configQuestion.Deserialize<MutipleChoiceQuestion>();
            int number = 0;
            if (dataAnswer != null && dataAnswer.Answers != null && dataQuestion != null && dataQuestion.Contents != null)
            {
                foreach (var item in dataAnswer.Answers)
                {
                    if (item.IsChecked)
                    {
                        var isCheck = dataQuestion.Contents.Any(x => x.IsCorrect == item.IsChecked);
                        if (isCheck)
                        {
                            number++;
                            item.IsExact = true;
                        }
                    }
                    else
                    {
                        item.IsExact = false;
                    }
                }
                return number;
            }
            return default;
        }

        private static int GetTotalCorrectTypeListingAnswer(object configAnswer, object configQuestion)
        {
            var dataAnswer = configAnswer.Deserialize<ListingAnswer>();
            var dataQuestion = configQuestion.Deserialize<ListingQuestion>();
            int number = 0;
            if (dataAnswer != null && dataAnswer.Answers != null && dataQuestion != null)
            {
                var answerStrs = dataAnswer.Answers.Split(' ');
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

        private static int GetTotalCorrectTypeShortAnswerWordCount(object configAnswer, object configQuestion)
        {
            var dataAnswer = configAnswer.Deserialize<ListingAnswer>();
            var dataQuestion = configQuestion.Deserialize<ListingQuestion>();
            int number = 0;
            if (dataAnswer != null && dataAnswer.Answers != null && dataQuestion != null)
            {
                var answerStrs = dataAnswer.Answers.Split(' ');
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

        private static int GetTotalCorrectTypeShortAnswerWordBase(object configAnswer, object configQuestion)
        {
            var dataAnswer = configAnswer.Deserialize<ListingAnswer>();
            var dataQuestion = configQuestion.Deserialize<ListingQuestion>();
            int number = 0;
            if (dataAnswer != null && dataAnswer.Answers != null && dataQuestion != null)
            {
                var answerStrs = dataAnswer.Answers.Split(' ');
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

        private static int GetTotalCorrectTypeGapFillGapAnswer(object configAnswer, object configQuestion)
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

        private static int GetTotalCorrectTypeGapFillBySubAnswer(object configAnswer, object configQuestion)
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

        private static int GetTotalCorrectTypeDragDropOrderAnswer(object configAnswer, object configQuestion)
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
                var answerStrs = dataAnswer.Answers.Split(' ');
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

        private static int GetTotalCorrectTypeDragDropPictureAnswer(object configAnswer, object configQuestion)
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
                var answerStrs = dataAnswer.Answers.Split(' ');
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
