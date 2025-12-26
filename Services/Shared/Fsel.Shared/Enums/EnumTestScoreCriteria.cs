// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Enums
{
    using System.ComponentModel;

    public enum EnumTestScoreCriteria
    {
        [Description("Fluency and Coherence (FC)")]
        FluencyAndCoherence,

        [Description("Lexical Resource (LR)")]
        LexicalResource,

        [Description("Grammatical Range and Accuracy (GRA)")]
        GrammaticalRangeAndAccuracy,

        [Description("Pronunciation (P)")]
        Pronunciation
    }
}
