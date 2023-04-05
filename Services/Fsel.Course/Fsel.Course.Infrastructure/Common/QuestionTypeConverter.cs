// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions;
    using Fsel.Course.Domain.Enums;

    public class QuestionTypeConverter
    {
        public object? QuestionTypeConverterObject(EnumQuestionType type, object? config, bool isShowOutcome = false)
        {
            switch (type)
            {
                case EnumQuestionType.Multichoice:
                case EnumQuestionType.Dropdown:
                case EnumQuestionType.Checklist:
                    var multichoice = config.Deserialize<MutipleChoiceQuestion>();
                    if (isShowOutcome)
                    {
                        return multichoice;
                    }
                    return ClearAnswerTypeMutipleChoiQuestion(multichoice);

                case EnumQuestionType.Listing:
                    var listingQuestion = config.Deserialize<ListingQuestion>();
                    if (isShowOutcome)
                    {
                        return listingQuestion;
                    }
                    return ClearAnswerTypeListingQuestion(listingQuestion);

                case EnumQuestionType.MatchingType1:
                case EnumQuestionType.MatchingType2:
                    var matchingTypeQuestion = config.Deserialize<MatchingTypeQuestion>();
                    if (isShowOutcome)
                    {
                        return matchingTypeQuestion;
                    }
                    return ClearAnswerTypeMaschingQuestion(matchingTypeQuestion);

                case EnumQuestionType.ShortAnswerWordBase:
                    var shortAnswerQuestionWordBaseQuestion = config.Deserialize<ShortAnswerQuestionWordBaseQuestion>();
                    if (isShowOutcome)
                    {
                        return shortAnswerQuestionWordBaseQuestion;
                    }
                    return ClearAnswersShortAnswer(shortAnswerQuestionWordBaseQuestion);

                case EnumQuestionType.ShortAnswerWordCount:
                    var shortAnswerWordCount = config.Deserialize<ShortAnswerQuestionWordCountBaseQuestion>();
                    if (isShowOutcome)
                    {
                        return shortAnswerWordCount;
                    }
                    return ClearAnswerTypeShortCountQuestion(shortAnswerWordCount);

                case EnumQuestionType.GapFillScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByGap:
                case EnumQuestionType.GapFillScoreByGap:
                    var gapFillQuestion = config.Deserialize<GapFillQuestion>();
                    if (isShowOutcome)
                    {
                        return gapFillQuestion;
                    }
                    return ClearAnswerTypeGapFillQuestion(gapFillQuestion);

                case EnumQuestionType.DragAndDropSentenceOrder:
                    var dragAndDropSentenceOrderQuestion = config.Deserialize<DragAndDropSentenceOrderQuestion>();
                    if (isShowOutcome)
                    {
                        return dragAndDropSentenceOrderQuestion;
                    }
                    return ClearAnswerTypeDragDropOrderQuestion(dragAndDropSentenceOrderQuestion);

                case EnumQuestionType.DragAndDropPicture:
                    var dragAndDropPictureQuestion = config.Deserialize<DragAndDropPictureQuestion>();
                    if (isShowOutcome)
                    {
                        return dragAndDropPictureQuestion;
                    }
                    return ClearAnswerTypeDragDropPictureQuestion(dragAndDropPictureQuestion);

                case EnumQuestionType.MultipleOptionSentenceCompletion:
                    var multipleOption = config.Deserialize<MultipleOptionSentenceCompletionQuestion>();
                    if (isShowOutcome)
                    {
                        return multipleOption;
                    }
                    return ClearAnswers(multipleOption);

                case EnumQuestionType.ExercisePreparation:
                    var data = config.Deserialize<ExercisePreparationQuestion>();
                    return data;

                default:
                    throw new ArgumentException("Invalid question type");
            }
        }

        private static object? ClearAnswers(MultipleOptionSentenceCompletionQuestion? data)
        {
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

        private static object? ClearAnswersShortAnswer(ShortAnswerQuestionWordBaseQuestion? data)
        {
            if (data != null)
            {
                data.Content = null;
            }
            return data;
        }

        private static object? ClearAnswerTypeListingQuestion(ListingQuestion? data)
        {
            if (data != null)
            {
                data.ExactWordCount = null;
            }
            return data;
        }

        private static object? ClearAnswerTypeMutipleChoiQuestion(MutipleChoiceQuestion? data)
        {
            if (data != null && data.Contents != null)
            {
                for (int i = data.Contents.Count - 1; i >= 0; i--)
                {
                    data.Contents[i].IsCorrect = default;
                }
            }
            return data;
        }

        private static object? ClearAnswerTypeShortCountQuestion(ShortAnswerQuestionWordCountBaseQuestion? data)
        {
            if (data != null)
            {
                data.ExactWordCount = null;
            }
            return data;
        }

        private static object? ClearAnswerTypeMaschingQuestion(MatchingTypeQuestion? data)
        {
            if (data != null && data.Link != null)
            {
                for (int i = data.Link.Count - 1; i >= 0; i--)
                {
                    data.Link.RemoveAt(i);
                }
            }
            return data;
        }

        private static object? ClearAnswerTypeGapFillQuestion(GapFillQuestion? data)
        {
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

        private static object? ClearAnswerTypeDragDropOrderQuestion(DragAndDropSentenceOrderQuestion? data)
        {
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

        private static object? ClearAnswerTypeDragDropPictureQuestion(DragAndDropPictureQuestion? data)
        {
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
    }
}
