// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions;
    using Fsel.Course.Domain.Enums;

    public static class EnumQuestionTypeCountConverter
    {
        public static int? GetTotalCorrectByQuestionType(this object config, EnumQuestionType type)
        {
            switch (type)
            {
                case EnumQuestionType.Multichoice:
                case EnumQuestionType.Dropdown:
                    return GetTotalCorrectTypeMultichoiceQuestion(config, type);

                case EnumQuestionType.Checklist:
                    return GetTotalCorrectTypeCheckListQuestion(config, type);

                case EnumQuestionType.Listing:
                    return GetTotalCorrectTypeListingQuestion(config, type);

                case EnumQuestionType.MatchingType1:
                case EnumQuestionType.MatchingType2:
                    return GetTotalCorrectTypeMaschingQuestion(config, type);

                case EnumQuestionType.ShortAnswerWordBase:
                    return GetTotalCorrectTypeShortBaseQuestion(config, type);

                case EnumQuestionType.ShortAnswerWordCount:
                    return GetTotalCorrectTypeShortCountQuestion(config, type);

                case EnumQuestionType.GapFillScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByGap:
                case EnumQuestionType.GapFillScoreByGap:
                    return GetTotalCorrectTypeGapFillQuestion(config, type);

                case EnumQuestionType.DragAndDropSentenceOrder:
                    return GetTotalCorrectTypeDragDropOrderQuestion(config, type);

                case EnumQuestionType.DragAndDropPicture:
                    return GetTotalCorrectTypeDragDropPictureQuestion(config, type);

                case EnumQuestionType.MultipleOptionSentenceCompletion:
                    return GetTotalCorrectTypeMultipleOptionQuestion(config, type);

                case EnumQuestionType.ExercisePreparation:
                    return GetTotalCorrectTypeExercisePreparationQuestion(config, type);

                default:
                    throw new ArgumentException("Invalid question type");
            }
        }

        public static int? GetTotalCorrectTypeMultipleOptionQuestion(object config, EnumQuestionType type)
        {
            if (config.TryParseQuestionType(type))
            {
                var data = config as MultipleOptionSentenceCompletionQuestion;
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
            }
            return null;
        }

        public static int? GetTotalCorrectTypeExercisePreparationQuestion(object config, EnumQuestionType type)
        {
            if (config.TryParseQuestionType(type))
            {
                return 0;
            }
            return null;
        }

        public static int? GetTotalCorrectTypeShortBaseQuestion(object config, EnumQuestionType type)
        {
            if (config.TryParseQuestionType(type))
            {
                return 1;
            }
            return null;
        }

        public static int? GetTotalCorrectTypeCheckListQuestion(object config, EnumQuestionType type)
        {
            if (config.TryParseQuestionType(type))
            {
                var data = config as MutipleChoiceQuestion;
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
            }
            return null;
        }

        public static int? GetTotalCorrectTypeListingQuestion(object config, EnumQuestionType type)
        {
            if (config.TryParseQuestionType(type))
            {
                return 1;
            }
            return null;
        }

        public static int? GetTotalCorrectTypeMultichoiceQuestion(object config, EnumQuestionType type)
        {
            if (config.TryParseQuestionType(type))
            {
                return 1;
            }
            return null;
        }

        public static int? GetTotalCorrectTypeShortCountQuestion(object config, EnumQuestionType type)
        {
            if (config.TryParseQuestionType(type))
            {
                return 1;
            }
            return null;
        }

        public static int? GetTotalCorrectTypeMaschingQuestion(object config, EnumQuestionType type)
        {
            if (config.TryParseQuestionType(type))
            {
                var data = config as MatchingTypeQuestion;
                int number = 0;
                if (data != null && data.Links != null)
                {
                    foreach (var item in data.Links)
                    {
                        number++;
                    }
                    return number;
                }
            }
            return null;
        }

        public static int? GetTotalCorrectTypeGapFillQuestion(object config, EnumQuestionType type)
        {
            if (config.TryParseQuestionType(type))
            {
                var data = config as GapFillQuestion;
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
            }
            return null;
        }

        public static int? GetTotalCorrectTypeDragDropOrderQuestion(object config, EnumQuestionType type)
        {
            if (config.TryParseQuestionType(type))
            {
                var data = config as DragAndDropSentenceOrderQuestion;
                int number = 0;
                if (data != null && data.Contents != null)
                {
                    foreach (var item in data.Contents)
                    {
                        number++;
                    }
                    return number;
                }
            }
            return null;
        }

        public static int? GetTotalCorrectTypeDragDropPictureQuestion(object config, EnumQuestionType type)
        {
            if (config.TryParseQuestionType(type))
            {
                var data = config as DragAndDropPictureQuestion;
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
            }
            return null;
        }
    }
}
