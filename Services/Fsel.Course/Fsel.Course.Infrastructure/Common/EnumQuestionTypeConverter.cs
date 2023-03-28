// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions;
    using Fsel.Course.Domain.Enums;

    public static class EnumQuestionTypeConverter
    {
        public static object? QuestionTypeConverter(EnumQuestionType type, object config)
        {
            switch (type)
            {
                case EnumQuestionType.Multichoice:
                case EnumQuestionType.Dropdown:
                case EnumQuestionType.Checklist:
                    return ClearAnswerTypeMutipleChoiQuestion(config, type);

                case EnumQuestionType.Listing:
                    return ClearAnswerTypeListingQuestion(config, type);

                case EnumQuestionType.MatchingType1:
                case EnumQuestionType.MatchingType2:
                    return ClearAnswerTypeMaschingQuestion(config, type);

                case EnumQuestionType.ShortAnswerWordBase:
                    return ClearAnswerTypeShortBaseQuestion(config, type);

                case EnumQuestionType.ShortAnswerWordCount:
                    return ClearAnswerTypeShortCountQuestion(config, type);

                case EnumQuestionType.GapFillScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByGap:
                case EnumQuestionType.GapFillScoreByGap:
                    return ClearAnswerTypeGapFillQuestion(config, type);

                case EnumQuestionType.DragAndDropSentenceOrder:
                    return ClearAnswerTypeDragDropOrderQuestion(config, type);

                case EnumQuestionType.DragAndDropPicture:
                    return ClearAnswerTypeDragDropPictureQuestion(config, type);

                case EnumQuestionType.MultipleOptionSentenceCompletion:
                    return ClearAnswerTypeMultipleOptionQuestion(config, type);

                case EnumQuestionType.ExercisePreparation:
                    return ClearAnswerTypeExercisePreparationQuestion(config, type);

                default:
                    throw new ArgumentException("Invalid question type");
            }
        }

        public static object? ClearAnswerTypeExercisePreparationQuestion(object config, EnumQuestionType type)
        {
            if (config.TryParseQuestionType(type))
            {
                var data = config as ExercisePreparationQuestion;
                if (data != null)
                {
                    return data;
                }
            }
            return null;
        }

        public static object? ClearAnswerTypeMultipleOptionQuestion(object config, EnumQuestionType type)
        {
            if (config.TryParseQuestionType(type))
            {
                var data = config as MultipleOptionSentenceCompletionQuestion;
                if (data != null && data.Contents != null)
                {
                    foreach (var item in data.Contents)
                    {
                        if (item != null)
                        {
                            item.Answers.ForEach(x =>
                            {
                                x.IsCorrect = null;
                            });
                        }
                    }
                }
                return data;
            }
            return null;
        }

        public static object? ClearAnswerTypeShortBaseQuestion(object config, EnumQuestionType type)
        {
            if (config.TryParseQuestionType(type))
            {
                var data = config as ShortAnswerQuestionWordBaseQuestion;
                if (data != null)
                {
                    data.Content = null;
                }
                return data;
            }
            return null;
        }

        public static object? ClearAnswerTypeListingQuestion(object config, EnumQuestionType type)
        {
            if (config.TryParseQuestionType(type))
            {
                var data = config as ListingQuestion;
                if (data != null)
                {
                    data.ExactWordCount = null;
                }
                return data;
            }
            return null;
        }

        public static object? ClearAnswerTypeMutipleChoiQuestion(object config, EnumQuestionType type)
        {
            if (config.TryParseQuestionType(type))
            {
                var data = config as MutipleChoiceQuestion;
                if (data != null && data.Contents != null)
                {
                    for (int i = data.Contents.Count - 1; i >= 0; i--)
                    {
                        data.Contents[i].IsCorrect = null;
                    }
                }
                return data;
            }
            return null;
        }

        public static object? ClearAnswerTypeShortCountQuestion(object config, EnumQuestionType type)
        {
            if (config.TryParseQuestionType(type))
            {
                var data = config as ShortAnswerQuestionWordCountBaseQuestion;
                if (data != null)
                {
                    data.ExactWordCount = null;
                }
                return data;
            }
            return null;
        }

        public static object? ClearAnswerTypeMaschingQuestion(object config, EnumQuestionType type)
        {
            if (config.TryParseQuestionType(type))
            {
                var data = config as MatchingTypeQuestion;
                if (data != null && data.Links != null)
                {
                    for (int i = data.Links.Count - 1; i >= 0; i--)
                    {
                        data.Links.RemoveAt(i);
                    }
                }
                return data;
            }
            return null;
        }

        public static object? ClearAnswerTypeGapFillQuestion(object config, EnumQuestionType type)
        {
            if (config.TryParseQuestionType(type))
            {
                var data = config as GapFillQuestion;
                if (data != null && data.Contents != null)
                {
                    foreach (var item in data.Contents)
                    {
                        item.Words.ForEach(y =>
                        {
                            item?.Words?.Remove(y);
                        });
                    }
                }
                return data;
            }
            return null;
        }

        public static object? ClearAnswerTypeDragDropOrderQuestion(object config, EnumQuestionType type)
        {
            if (config.TryParseQuestionType(type))
            {
                var data = config as DragAndDropSentenceOrderQuestion;
                if (data != null && data.Contents != null)
                {
                    foreach (var item in data.Contents)
                    {
                        item.Words.ForEach(y =>
                        {
                            item?.Words?.Remove(y);
                        });
                    }
                }
                return data;
            }
            return null;
        }

        public static object? ClearAnswerTypeDragDropPictureQuestion(object config, EnumQuestionType type)
        {
            if (config.TryParseQuestionType(type))
            {
                var data = config as DragAndDropPictureQuestion;
                if (data != null && data.Contents != null)
                {
                    foreach (var item in data.Contents)
                    {
                        item.Words.ForEach(y =>
                        {
                            y.Content = null;
                        });
                    }
                }
                return data;
            }
            return null;
        }
    }
}
