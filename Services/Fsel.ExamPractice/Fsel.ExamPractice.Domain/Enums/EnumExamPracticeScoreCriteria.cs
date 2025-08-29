// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Enums
{
    using System.ComponentModel;

    public enum EnumExamPracticeScoreCriteria
    {
        [Description("Fluency and Coherence (FC)")]
        FluencyAndCoherence,

        [Description("Lexical Resource (LR)")]
        LexicalResource,

        [Description("Grammatical Range and Accuracy (GRA)")]
        GrammaticalRangeAndAccuracy,

        [Description("Pronunciation (P)")]
        Pronunciation,

        [Description("Discourse Management (DM)")]
        DiscourseManagement,

        [Description("Vocabulary")]
        Vocabulary
    }
}
