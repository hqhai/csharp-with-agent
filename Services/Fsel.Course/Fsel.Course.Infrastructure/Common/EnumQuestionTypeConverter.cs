// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions;
    using Fsel.Course.Domain.Enums;

    public static class EnumQuestionTypeConverter
    {
        public static bool TryParse<T>(this object? config, out T? result)
        {
            var str = config.Serialize();
            result = str.Deserialize<T>(true);
            if (result == null)
            {
                return false;
            }
            return true;
        }

        public static bool TryParse<T>(this string? str, out T? result)
        {
            result = str.Deserialize<T>(true);
            if (result == null)
            {
                return false;
            }
            return true;
        }

        public static object? QuestionTypeConverter(EnumQuestionType type, object config)
        {
            switch (type)
            {
                case EnumQuestionType.Multichoice:
                case EnumQuestionType.Dropdown:
                case EnumQuestionType.Checklist:
                    return ConverTypeQuestion(config);

                case EnumQuestionType.Listing:
                    return ConverTypeListingQuestion(config);

                case EnumQuestionType.MatchingType1:
                case EnumQuestionType.MatchingType2:
                    return ConverTypeMaschingQuestion(config);

                case EnumQuestionType.ShortAnswerWordBase:
                    return ConverTypeShortBaseQuestion(config);

                case EnumQuestionType.ShortAnswerWordCount:
                    return ConverTypeShortCountQuestion(config);

                case EnumQuestionType.GapFillScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByGap:
                case EnumQuestionType.GapFillScoreByGap:
                    return ConverTypeGapFillQuestion(config);

                case EnumQuestionType.DragAndDropSentenceOrder:
                    return ConverTypeDragDropOrderQuestion(config);

                case EnumQuestionType.DragAndDropPicture:
                    return ConverTypeDragDropPictureQuestion(config);

                case EnumQuestionType.ExercisePreparation:
                    return ConverTypeQuestion(config);

                default:
                    throw new ArgumentException("Invalid question type");
            }
        }

        public static object? ConverTypeShortBaseQuestion(object config)
        {
            if (config.TryParse<ShortAnswerQuestionWordBaseQuestion>(out var question))
            {
                question.Content = null;
                return question;
            }
            return null;
        }

        public static object? ConverTypeListingQuestion(object config)
        {
            if (config.TryParse<ListingQuestion>(out var question))
            {
                question.ExactWordCount = null;
                return question;
            }
            return null;
        }

        public static object? ConverTypeQuestion(object config)
        {
            if (config.TryParse<MutipleChoiceQuestion>(out var question))
            {
                for (int i = question.Contents.Count - 1; i >= 0; i--)
                {
                    question.Contents[i].IsCorrect = null;
                }
                return question;
            }
            return null;
        }

        public static object? ConverTypeShortCountQuestion(object config)
        {
            if (config.TryParse<ShortAnswerQuestionWordCountBaseQuestion>(out var question))
            {
                question.ExactWordCount = null;
                return question;
            }
            return null;
        }

        public static object? ConverTypeMaschingQuestion(object config)
        {
            if (config.TryParse<MatchingTypeQuestion>(out var question))
            {
                for (int i = question.Links.Count - 1; i >= 0; i--)
                {
                    question.Links.RemoveAt(i);
                }
                return question;
            }
            return null;
        }

        public static object? ConverTypeGapFillQuestion(object config)
        {
            if (config.TryParse<GapFillQuestion>(out var question))
            {
                foreach (var item in question.Contents)
                {
                    item.Words.ForEach(y =>
                    {
                        item?.Words?.Remove(y);
                    });
                }
                return question;
            }
            return null;
        }

        public static object? ConverTypeDragDropOrderQuestion(object config)
        {
            if (config.TryParse<DragAndDropSentenceOrderQuestion>(out var question))
            {
                foreach (var item in question.Contents)
                {
                    item.Words.ForEach(y =>
                    {
                        item?.Words?.Remove(y);
                    });
                }
                return question;
            }
            return null;
        }

        public static object? ConverTypeDragDropPictureQuestion(object config)
        {
            if (config.TryParse<DragAndDropPictureQuestion>(out var question))
            {
                foreach (var item in question?.Contents)
                {
                    item.Words.ForEach(y =>
                    {
                        y.Content = null;
                    });
                }
                return question;
            }
            return null;
        }
    }
}
