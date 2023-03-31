// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions;
    using Fsel.Course.Domain.Enums;

    public class QuestionTypeCountConverter
    {
        public int? GetTotalCorrectByQuestionType(object? config, EnumQuestionType type)
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
                    return GetTotalCorrectTypeGapFillBySubQuestion(config);

                case EnumQuestionType.GapFillWordBankScoreByGap:
                case EnumQuestionType.GapFillScoreByGap:
                    return GetTotalCorrectTypeGapFillByGap(config);

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

        private static int? GetTotalCorrectTypeMultipleOptionQuestion(object? config)
        {
            var data = config.Deserialize<MultipleOptionSentenceCompletionQuestion>();
            if (data != null && data.Contents != null)
            {
                return data.Contents.Sum(x => x.Answers?.Count);
            }
            return default;
        }

        private static int? GetTotalCorrectTypeCheckListQuestion(object? config)
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
                return data.Contents.Where(x => x.IsCorrect == true).Count();
            }
            return default;
        }

        private static int? GetTotalCorrectTypeDefaultQuestion()
        {
            return 1;
        }

        private static int? GetTotalCorrectTypeMaschingQuestion(object? config)
        {
            var data = config.Deserialize<MatchingTypeQuestion>();
            if (data != null && data.Link != null)
            {
                return data.Link.Count;
            }
            return default;
        }

        private static int? GetTotalCorrectTypeGapFillBySubQuestion(object? config)
        {
            var data = config.Deserialize<GapFillQuestion>();
            if (data != null && data.Contents != null)
            {
                return data.Contents.Count;
            }
            return default;
        }

        private static int? GetTotalCorrectTypeGapFillByGap(object? config)
        {
            var data = config.Deserialize<GapFillQuestion>();
            if (data != null && data.Contents != null)
            {
                return data.Contents.Sum(x => x.Words?.Count);
            }
            return default;
        }

        private static int? GetTotalCorrectTypeDragDropOrderQuestion(object? config)
        {
            var data = config.Deserialize<DragAndDropSentenceOrderQuestion>();
            if (data != null && data.Contents != null)
            {
                return data.Contents.Count;
            }
            return default;
        }

        private static int? GetTotalCorrectTypeDragDropPictureQuestion(object? config)
        {
            var data = config.Deserialize<DragAndDropPictureQuestion>();
            if (data != null && data.Contents != null)
            {
                return data.Contents.Sum(x => x?.Images?.Count);
            }
            return default;
        }
    }
}
