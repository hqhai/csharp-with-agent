// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Enums
{
    using System.ComponentModel;

    public enum EnumQuestionType
    {
        [Description("Multiple Choice")]
        Multichoice = 1,

        [Description("Drop down")]
        Dropdown,

        [Description("Checklist")]
        Checklist,

        [Description("Listing")]
        Listing,

        [Description("Matching (type 1)")]
        MatchingType1,

        [Description("Matching (type 2)")]
        MatchingType2,

        [Description("Short answer (Answer base)")]
        ShortAnswerWordBase,

        [Description("Short answer (Word count)")]
        ShortAnswerWordCount,

        [Description("Gap fill (score by sub question)")]
        GapFillScoreByQuestion,

        [Description("Gap fill with word bank (score by sub question)")]
        GapFillWordBankScoreByQuestion,

        [Description("Gap fill (score by gap)")]
        GapFillScoreByGap,

        [Description("Gap fill with word bank (score by gap)")]
        GapFillWordBankScoreByGap,

        [Description("Drag and Drop (Sentence Order)")]
        DragAndDropSentenceOrder,

        [Description("Drag and Drop (Picture)")]
        DragAndDropPicture,

        [Description("Multiple option Sentence Completion")]
        MultipleOptionSentenceCompletion,

        [Description("Exercise preparation")]
        ExercisePreparation
    }
}
