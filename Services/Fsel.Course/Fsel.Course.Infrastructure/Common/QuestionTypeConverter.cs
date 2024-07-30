// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions;
    using Fsel.Shared.Enums;

    public class QuestionTypeConverter
    {
        public (object?, int) QuestionTypeConverterObject(object? config, EnumQuestionType type, bool isShowCorrectTotal = false, bool isDisableAnswers = false)
        {
            int totalCorrect = default;
            object? result;
            switch (type)
            {
                case EnumQuestionType.Multichoice:
                case EnumQuestionType.Dropdown:
                    var multichoice = config.Deserialize<MultipleChoiceQuestion>();
                    result = isDisableAnswers ? ClearAnswers(multichoice) : multichoice;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect() : default;
                    break;

                case EnumQuestionType.Checklist:
                    var checklist = config.Deserialize<MultipleChoiceQuestion>();
                    result = isDisableAnswers ? ClearAnswers(checklist) : checklist;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(checklist) : default;
                    break;

                case EnumQuestionType.Listing:
                    result = config.Deserialize<ListingQuestion>();
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect() : default;
                    break;

                case EnumQuestionType.MatchingType1:
                case EnumQuestionType.MatchingType2:
                case EnumQuestionType.DragAndDropPicture:
                    var matchingTypeQuestion = config.Deserialize<MatchingTypeQuestion>();
                    result = isDisableAnswers ? ClearAnswers(matchingTypeQuestion) : matchingTypeQuestion;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(matchingTypeQuestion) : default;
                    break;

                case EnumQuestionType.ShortAnswerWordBase:
                    var shortAnswerQuestionWordBaseQuestion = config.Deserialize<ShortAnswerQuestionWordBaseQuestion>();
                    result = isDisableAnswers ? ClearAnswers(shortAnswerQuestionWordBaseQuestion) : shortAnswerQuestionWordBaseQuestion;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect() : default;
                    break;

                case EnumQuestionType.ShortAnswerWordCount:
                    result = config.Deserialize<ShortAnswerQuestionWordCountBaseQuestion>();
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect() : default;
                    break;

                case EnumQuestionType.GapFillScoreByQuestion:
                    var gapFillQuestion = config.Deserialize<GapFillQuestion>();
                    result = isDisableAnswers ? ClearAnswers(gapFillQuestion) : gapFillQuestion;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrectBySubQuestion(gapFillQuestion) : default;
                    break;

                case EnumQuestionType.GapFillWordBankScoreByQuestion:
                    var gapFillWordBankScoreQuestion = config.Deserialize<GapFillQuestion>();
                    result = isDisableAnswers ? ClearAnswers(gapFillWordBankScoreQuestion) : gapFillWordBankScoreQuestion;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrectBySubQuestion(gapFillWordBankScoreQuestion) : default;
                    break;

                case EnumQuestionType.GapFillWordBankScoreByGap:
                    var gapFillWordBankScoreByGap = config.Deserialize<GapFillQuestion>();
                    result = isDisableAnswers ? ClearAnswers(gapFillWordBankScoreByGap) : gapFillWordBankScoreByGap;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrectByGap(gapFillWordBankScoreByGap) : default;
                    break;

                case EnumQuestionType.GapFillScoreByGap:
                    var gapFillQuestionByGap = config.Deserialize<GapFillQuestion>();
                    result = isDisableAnswers ? ClearAnswers(gapFillQuestionByGap) : gapFillQuestionByGap;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrectByGap(gapFillQuestionByGap) : default;
                    break;

                case EnumQuestionType.DragAndDropSentenceOrder:
                    var dragAndDropSentenceOrderQuestion = config.Deserialize<DragAndDropSentenceOrderQuestion>();
                    result = isDisableAnswers ? ClearAnswers(dragAndDropSentenceOrderQuestion) : dragAndDropSentenceOrderQuestion;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(dragAndDropSentenceOrderQuestion) : default;
                    break;

                case EnumQuestionType.DragAndDropListSentenceOrder:
                    var dragAndDropList = config.Deserialize<DragAndDropListSentenceOrderQuestion>();
                    if (dragAndDropList != null)
                    {
                        dragAndDropList.Contents = dragAndDropList.Contents?.Select((x, index) => { x.Id = ++index; return x; }).ToList();
                    }
                    result = isDisableAnswers ? ClearAnswers(dragAndDropList) : dragAndDropList;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect() : default;
                    break;

                case EnumQuestionType.MultipleOptionSentenceCompletion:
                    var multipleOption = config.Deserialize<MultipleOptionSentenceCompletionQuestion>();
                    result = isDisableAnswers ? ClearAnswers(multipleOption) : multipleOption;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(multipleOption) : default;
                    break;

                case EnumQuestionType.ExercisePreparation:
                    result = config.Deserialize<ExercisePreparationQuestion>();
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect() : default;
                    break;

                default:
                    throw new ArgumentException("Invalid question type");
            }

            return (result, totalCorrect);
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

        private static object? ClearAnswers(ShortAnswerQuestionWordBaseQuestion? data)
        {
            if (data != null && data.Content != null && data.Content.Any())
            {
                data.Content.Clear();
            }
            return data;
        }

        private static object? ClearAnswers(MultipleChoiceQuestion? data)
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

        private static object? ClearAnswers(DragAndDropSentenceOrderQuestion? data)
        {
            if (data != null && data.Contents != null)
            {
                foreach (var item in data.Contents)
                {
                    item.Words = GenerateRandomLoop(item.Words);
                }
            }
            return data;
        }

        private static DragAndDropListSentenceOrderQuestion? ClearAnswers(DragAndDropListSentenceOrderQuestion? data)
        {
            var contents = GenerateRandomLoop(data?.Contents);
            if (data != null)
            {
                data.Contents = contents;
            }
            return data;
        }

        private static object? ClearAnswers(MatchingTypeQuestion? data)
        {
            if (data != null && data.Link != null)
            {
                data.Link.Clear();
            }
            return data;
        }

        private static object? ClearAnswers(GapFillQuestion? data)
        {
            if (data != null && data.Contents != null)
            {
                foreach (var item in data.Contents)
                {
                    item.Words = GenerateRandomLoop(item.Words);
                }
            }
            return data;
        }

        private static int GetTotalCorrect(MultipleOptionSentenceCompletionQuestion? data)
        {
            if (data != null && data.Contents != null)
            {
                return data.Contents.Count;
            }
            return default;
        }

        private static int GetTotalCorrect(MultipleChoiceQuestion? data)
        {
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
                return data.Contents.Where(x => x.IsCorrect == true).Count();
            }
            return default;
        }

        private static int GetTotalCorrect()
        {
            return 1;
        }

        private static int GetTotalCorrect(MatchingTypeQuestion? data)
        {
            if (data != null && data.Link != null)
            {
                return data.Link.Count;
            }
            return default;
        }

        private static int GetTotalCorrectBySubQuestion(GapFillQuestion? data)
        {
            if (data != null && data.Contents != null)
            {
                return data.Contents.Count;
            }
            return default;
        }

        private static int GetTotalCorrectByGap(GapFillQuestion? data)
        {
            if (data != null && data.Contents != null)
            {
                return data.Contents.Sum(x => x.Words?.Count ?? default);
            }
            return default;
        }

        private static int GetTotalCorrect(DragAndDropSentenceOrderQuestion? data)
        {
            if (data != null && data.Contents != null)
            {
                return data.Contents.Count;
            }
            return default;
        }

        private static IList<T>? GenerateRandomLoop<T>(IList<T>? datas)
        {
            var rand = new Random();
            if (datas != null)
            {
                return datas.OrderBy(_ => rand.Next()).ToList();
            }
            return datas;
        }
    }
}
