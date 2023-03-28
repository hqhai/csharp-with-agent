// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions;
    using Fsel.Course.Domain.Enums;

    public static class EnumQuestionTypeValidation
    {
        public static bool TryParseQuestionType(this object? config, EnumQuestionType type)
        {
            bool check;
            switch (type)
            {
                case EnumQuestionType.Multichoice:
                case EnumQuestionType.Dropdown:
                case EnumQuestionType.Checklist:
                    check = config.TryParse<MutipleChoiceQuestion>(out var multichoice);
                    config = multichoice;
                    break;

                case EnumQuestionType.Listing:
                    check = config.TryParse<ListingQuestion>(out var listing);
                    config = listing;
                    break;

                case EnumQuestionType.MatchingType1:
                case EnumQuestionType.MatchingType2:
                    check = config.TryParse<MatchingTypeQuestion>(out var matchingType);
                    config = matchingType;
                    break;

                case EnumQuestionType.ShortAnswerWordBase:
                    check = config.TryParse<ShortAnswerQuestionWordBaseQuestion>(out var shortAnswerWordBase);
                    config = shortAnswerWordBase;
                    break;

                case EnumQuestionType.ShortAnswerWordCount:
                    check = config.TryParse<ShortAnswerQuestionWordCountBaseQuestion>(out var shortAnswerWordCount);
                    config = shortAnswerWordCount;
                    break;

                case EnumQuestionType.GapFillScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByQuestion:
                case EnumQuestionType.GapFillWordBankScoreByGap:
                case EnumQuestionType.GapFillScoreByGap:
                    check = config.TryParse<GapFillQuestion>(out var gapFillQuestion);
                    config = gapFillQuestion;
                    break;

                case EnumQuestionType.DragAndDropSentenceOrder:
                    check = config.TryParse<DragAndDropSentenceOrderQuestion>(out var dragAndDropSentence);
                    config = dragAndDropSentence;
                    break;

                case EnumQuestionType.DragAndDropPicture:
                    check = config.TryParse<DragAndDropPictureQuestion>(out var dragAndDropPicture);
                    config = dragAndDropPicture;
                    break;

                case EnumQuestionType.MultipleOptionSentenceCompletion:
                    check = config.TryParse<MultipleOptionSentenceCompletionQuestion>(out var multipleOption);
                    config = multipleOption;
                    break;

                case EnumQuestionType.ExercisePreparation:
                    check = config.TryParse<ExercisePreparationQuestion>(out var exercisePreparation);
                    config = exercisePreparation;
                    break;

                default:
                    throw new ArgumentException("Invalid question type");
            }

            return check;
        }
    }
}
