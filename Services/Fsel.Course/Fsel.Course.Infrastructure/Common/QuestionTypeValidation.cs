// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions;
    using Fsel.Course.Domain.Enums;

    public class QuestionTypeValidation
    {
        public async Task<bool> TryParseQuestionTypeAsync(object? config, EnumQuestionType type)
        {
            switch (type)
            {
                case EnumQuestionType.Multichoice:
                case EnumQuestionType.Dropdown:
                case EnumQuestionType.Checklist:
                    return config.Deserialize<MutipleChoiceQuestion>() != null;

                case EnumQuestionType.Listing:
                    return config.Deserialize<ListingQuestion>() != null;

                case EnumQuestionType.MatchingType1:
                case EnumQuestionType.MatchingType2:
                    return config.Deserialize<MatchingTypeQuestion>() != null;

                case EnumQuestionType.ShortAnswerWordBase:
                    return config.Deserialize<ShortAnswerQuestionWordBaseQuestion>() != null;

                case EnumQuestionType.ShortAnswerWordCount:
                    return config.Deserialize<ShortAnswerQuestionWordCountBaseQuestion>() != null;

                case EnumQuestionType.GapFillScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByGap:
                case EnumQuestionType.GapFillScoreByGap:
                    return config.Deserialize<GapFillQuestion>() != null;

                case EnumQuestionType.DragAndDropSentenceOrder:
                    return config.Deserialize<DragAndDropSentenceOrderQuestion>() != null;

                case EnumQuestionType.DragAndDropPicture:
                    return config.Deserialize<DragAndDropPictureQuestion>() != null;

                case EnumQuestionType.MultipleOptionSentenceCompletion:
                    return config.Deserialize<MultipleOptionSentenceCompletionQuestion>() != null;

                case EnumQuestionType.ExercisePreparation:
                    return config.Deserialize<ExercisePreparationQuestion>() != null;

                default:
                    throw new ArgumentException("Invalid question type");
            }
        }
    }
}
