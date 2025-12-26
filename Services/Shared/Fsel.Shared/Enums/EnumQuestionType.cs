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

        [Description("Drag and Drop (List Sentence Order)")]
        DragAndDropListSentenceOrder,

        [Description("Drag and Drop (Sentence Order)")]
        DragAndDropSentenceOrder,

        [Description("Drag and Drop (Picture)")]
        DragAndDropPicture,

        [Description("Multiple option Sentence Completion")]
        MultipleOptionSentenceCompletion,

        [Description("Exercise preparation")]
        ExercisePreparation,

        // Dạng hỏi mới

        [Description("Multiple Choice")]
        MultichoiceV1,

        [Description("Check list")]
        CheckListV1,

        [Description("Summary completion (Gap fill)")]
        SummaryCompletionGapFill,

        [Description("Completion Diagrams")]
        CompletionDiagrams,

        [Description("Yes/No/Not given (Drop down)")]
        YesNoNotGivenDropDown,

        [Description("True/False/Not given (Drop down)")]
        TrueFalseNotGivenDropDown,

        [Description("Map labeling (Drop down)")]
        MapLabelingDropDown,

        [Description("Summary completion (Drop down)")]
        SummaryCompletionDropDown,

        [Description("Matching paragraph info")]
        MatchingParagraphInfo,

        [Description("Matching heading")]
        MatchingHeading,

        [Description("Table Completion")]
        TableCompletion,

        [Description("Flow chart completion")]
        FlowChartCompletion,

        [Description("Tracing")]
        Tracing,

        [Description("Color Matching Type")]
        ColorMatchingType,

        [Description("Long Answer")]
        LongAnswer,
    }
}
