// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Enums
{
    using System.ComponentModel;

    public enum EnumSectionExamPracticeType
    {
        [Description("Pronunciation")]
        Pronunciation,

        [Description("Stress")]
        Stress,

        [Description("Grammar")]
        Grammar,

        [Description("Gap Fill")]
        GapFill,

        [Description("Synonym")]
        Synonym,

        [Description("Antonym")]
        Antonym,

        [Description("Exchange")]
        Exchange,

        [Description("Reading Comprehension")]
        ReadingComprehension,

        [Description("Reading Gap Fill")]
        ReadingGapFill,

        [Description("Grammar Correction")]
        GrammarCorrection,

        [Description("Paraphrasing")]
        Paraphrasing,

        [Description("Best Written")]
        BestWritten,

        [Description("Announcement Gap Fill")]
        AnnouncementGapFill,

        [Description("Coherence & Completion")]
        CoherenceCompletion,

        [Description("Sign")]
        Sign,

        [Description("Notice")]
        Notice,

        [Description("Phrase Replacement")]
        PhraseReplacement,

        [Description("Advertisement Grammar Gap Fill")]
        AdvertisementGrammarGapFill,

        [Description("Leaflet Grammar Gap Fill")]
        LeafletGrammarGapFill,

        [Description("Utterance Arrangement")]
        UtteranceArrangement,

        [Description("Text Completion")]
        TextCompletion,

        [Description("Text - Lexical Comprehension")]
        TextLexicalComprehension,

        [Description("Text - Paraphrasing")]
        TextParaphrasing
    }
}
