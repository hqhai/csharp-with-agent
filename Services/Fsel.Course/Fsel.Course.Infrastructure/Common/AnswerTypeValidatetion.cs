// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers;
    using Fsel.Course.Domain.Enums;

    public class AnswerTypeValidatetion
    {
        public bool TryParseAnswerType(object? config, EnumQuestionType type)
        {
            switch (type)
            {
                case EnumQuestionType.Multichoice:
                case EnumQuestionType.Dropdown:
                case EnumQuestionType.Checklist:
                    return config.Deserialize<MutipleChoiceAnswer>() != null;

                case EnumQuestionType.Listing:
                    return config.Deserialize<ListingAnswer>() != null;

                case EnumQuestionType.MatchingType1:
                case EnumQuestionType.MatchingType2:
                    return config.Deserialize<MatchingTypeAnswer>() != null;

                case EnumQuestionType.ShortAnswerWordBase:
                    return config.Deserialize<ShortAnswerWordBaseAnswer>() != null;

                case EnumQuestionType.ShortAnswerWordCount:
                    return config.Deserialize<ShortAnswerWordCountBaseAnswer>() != null;

                case EnumQuestionType.GapFillScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByGap:
                case EnumQuestionType.GapFillScoreByGap:
                    return config.Deserialize<GapFillAnswer>() != null;

                case EnumQuestionType.DragAndDropSentenceOrder:
                    return config.Deserialize<DragAndDropSentenceOrderAnswer>() != null;

                case EnumQuestionType.DragAndDropPicture:
                    return config.Deserialize<DragAndDropPictureAnswer>() != null;

                case EnumQuestionType.MultipleOptionSentenceCompletion:
                    return config.Deserialize<MultipleOptionSentenceCompletionAnswer>() != null;

                default:
                    throw new ArgumentException("Invalid answer type");
            }
        }
    }
}
