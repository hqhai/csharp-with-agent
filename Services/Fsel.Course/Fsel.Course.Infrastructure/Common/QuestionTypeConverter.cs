// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions;
    using Fsel.Course.Domain.Enums;

    public class QuestionTypeConverter
    {
        public object? QuestionTypeConverterObject(EnumQuestionType type, object config)
        {
            switch (type)
            {
                case EnumQuestionType.Multichoice:
                case EnumQuestionType.Dropdown:
                case EnumQuestionType.Checklist:
                    return ClearAnswerTypeMutipleChoiQuestion(config);

                case EnumQuestionType.Listing:
                    return ClearAnswerTypeListingQuestion(config);

                case EnumQuestionType.MatchingType1:
                case EnumQuestionType.MatchingType2:
                    return ClearAnswerTypeMaschingQuestion(config);

                case EnumQuestionType.ShortAnswerWordBase:
                    return ClearAnswerTypeShortBaseQuestion(config);

                case EnumQuestionType.ShortAnswerWordCount:
                    return ClearAnswerTypeShortCountQuestion(config);

                case EnumQuestionType.GapFillScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByGap:
                case EnumQuestionType.GapFillScoreByGap:
                    return ClearAnswerTypeGapFillQuestion(config);

                case EnumQuestionType.DragAndDropSentenceOrder:
                    return ClearAnswerTypeDragDropOrderQuestion(config);

                case EnumQuestionType.DragAndDropPicture:
                    return ClearAnswerTypeDragDropPictureQuestion(config);

                case EnumQuestionType.MultipleOptionSentenceCompletion:
                    return ClearAnswerTypeMultipleOptionQuestion(config);

                case EnumQuestionType.ExercisePreparation:
                    return ClearAnswerTypeExercisePreparationQuestion(config);

                default:
                    throw new ArgumentException("Invalid question type");
            }
        }

        private static object? ClearAnswerTypeExercisePreparationQuestion(object config)
        {
            var data = config.Deserialize<ExercisePreparationQuestion>();
            if (data != null)
            {
                return data;
            }
            return null;
        }

        private static object? ClearAnswerTypeMultipleOptionQuestion(object config)
        {
            var data = config.Deserialize<MultipleOptionSentenceCompletionQuestion>();
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
                return data;
            }
            return null;
        }

        private static object? ClearAnswerTypeShortBaseQuestion(object config)
        {
            var data = config.Deserialize<ShortAnswerQuestionWordBaseQuestion>();
            if (data != null)
            {
                data.Content = null;
                return data;
            }
            return null;
        }

        private static object? ClearAnswerTypeListingQuestion(object config)
        {
            var data = config.Deserialize<ListingQuestion>();
            if (data != null)
            {
                data.ExactWordCount = null;
                return data;
            }
            return null;
        }

        private static object? ClearAnswerTypeMutipleChoiQuestion(object config)
        {
            var data = config.Deserialize<MutipleChoiceQuestion>();
            if (data != null && data.Contents != null)
            {
                for (int i = data.Contents.Count - 1; i >= 0; i--)
                {
                    data.Contents[i].IsCorrect = default;
                }
                return data;
            }
            return null;
        }

        private static object? ClearAnswerTypeShortCountQuestion(object config)
        {
            var data = config.Deserialize<ShortAnswerQuestionWordCountBaseQuestion>();
            if (data != null)
            {
                data.ExactWordCount = null;
                return data;
            }
            return null;
        }

        private static object? ClearAnswerTypeMaschingQuestion(object config)
        {
            var data = config.Deserialize<MatchingTypeQuestion>();
            if (data != null && data.Link != null)
            {
                for (int i = data.Link.Count - 1; i >= 0; i--)
                {
                    data.Link.RemoveAt(i);
                }
                return data;
            }
            return null;
        }

        private static object? ClearAnswerTypeGapFillQuestion(object config)
        {
            var data = config.Deserialize<GapFillQuestion>();
            if (data != null && data.Contents != null)
            {
                foreach (var item in data.Contents)
                {
                    item.Words.ForEach(y =>
                    {
                        item?.Words?.Remove(y);
                    });
                }
                return data;
            }
            return null;
        }

        private static object? ClearAnswerTypeDragDropOrderQuestion(object config)
        {
            var data = config.Deserialize<DragAndDropSentenceOrderQuestion>();
            if (data != null && data.Contents != null)
            {
                foreach (var item in data.Contents)
                {
                    item.Words.ForEach(y =>
                    {
                        item?.Words?.Remove(y);
                    });
                }
                return data;
            }
            return null;
        }

        private static object? ClearAnswerTypeDragDropPictureQuestion(object config)
        {
            var data = config.Deserialize<DragAndDropPictureQuestion>();
            if (data != null && data.Contents != null)
            {
                foreach (var item in data.Contents)
                {
                    item.Words.ForEach(y =>
                    {
                        y.Content = null;
                    });
                }
                return data;
            }
            return null;
        }
    }
}
