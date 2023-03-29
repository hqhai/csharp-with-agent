// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions;
    using Fsel.Course.Domain.Enums;

    public class QuestionTypeCountConverter
    {
        public int GetTotalCorrectByQuestionType(object config, EnumQuestionType type)
        {
            switch (type)
            {
                case EnumQuestionType.Multichoice:
                case EnumQuestionType.Dropdown:
                    return GetTotalCorrectTypeDefaultQuestion();

                case EnumQuestionType.Checklist:
                    return GetTotalCorrectTypeCheckListQuestion(config);

                case EnumQuestionType.Listing:
                    return GetTotalCorrectTypeDefaultQuestion();

                case EnumQuestionType.MatchingType1:
                case EnumQuestionType.MatchingType2:
                    return GetTotalCorrectTypeMaschingQuestion(config);

                case EnumQuestionType.ShortAnswerWordBase:
                    return GetTotalCorrectTypeDefaultQuestion();

                case EnumQuestionType.ShortAnswerWordCount:
                    return GetTotalCorrectTypeDefaultQuestion();

                case EnumQuestionType.GapFillScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByGap:
                case EnumQuestionType.GapFillScoreByGap:
                    return GetTotalCorrectTypeGapFillQuestion(config);

                case EnumQuestionType.DragAndDropSentenceOrder:
                    return GetTotalCorrectTypeDragDropOrderQuestion(config);

                case EnumQuestionType.DragAndDropPicture:
                    return GetTotalCorrectTypeDragDropPictureQuestion(config);

                case EnumQuestionType.MultipleOptionSentenceCompletion:
                    return GetTotalCorrectTypeMultipleOptionQuestion(config);

                case EnumQuestionType.ExercisePreparation:
                    return default;

                default:
                    throw new ArgumentException("Invalid question type");
            }
        }

        private static int GetTotalCorrectTypeMultipleOptionQuestion(object config)
        {
            var data = config.Deserialize<MultipleOptionSentenceCompletionQuestion>();
            int number = 0;
            if (data != null && data.Contents != null)
            {
                foreach (var item in data.Contents)
                {
                    if (item != null)
                    {
                        item.Answers.ForEach(x =>
                        {
                            number++;
                        });
                    }
                }
                return number;
            }
            return default;
        }

        private static int GetTotalCorrectTypeCheckListQuestion(object config)
        {
            var data = config.Deserialize<MutipleChoiceQuestion>();
            int number = 0;
            if (data != null && data.Contents != null)
            {
                foreach (var item in data.Contents)
                {
                    if (item.IsCorrect == true)
                    {
                        number++;
                    }
                }
                return number;
            }
            return default;
        }

        private static int GetTotalCorrectTypeDefaultQuestion()
        {
            return 1;
        }

        private static int GetTotalCorrectTypeMaschingQuestion(object config)
        {
            var data = config.Deserialize<MatchingTypeQuestion>();
            int number = 0;
            if (data != null && data.Links != null)
            {
                foreach (var item in data.Links)
                {
                    number++;
                }
                return number;
            }
            return default;
        }

        private static int GetTotalCorrectTypeGapFillQuestion(object config)
        {
            var data = config.Deserialize<GapFillQuestion>();
            int number = 0;
            if (data != null && data.Contents != null)
            {
                foreach (var item in data.Contents)
                {
                    item.Words.ForEach(x =>
                    {
                        number++;
                    });
                }
                return number;
            }
            return default;
        }

        private static int GetTotalCorrectTypeDragDropOrderQuestion(object config)
        {
            var data = config.Deserialize<DragAndDropSentenceOrderQuestion>();
            int number = 0;
            if (data != null && data.Contents != null)
            {
                foreach (var item in data.Contents)
                {
                    number++;
                }
                return number;
            }
            return default;
        }

        private static int GetTotalCorrectTypeDragDropPictureQuestion(object config)
        {
            var data = config.Deserialize<DragAndDropPictureQuestion>();
            int number = 0;
            if (data != null && data.Contents != null)
            {
                foreach (var item in data.Contents)
                {
                    item.Images.ForEach(y =>
                    {
                        number++;
                    });
                }
                return number;
            }
            return default;
        }
    }
}
