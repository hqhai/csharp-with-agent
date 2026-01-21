// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Enums
{
    using global::System.ComponentModel;

    public enum EnumSubFeatureType
    {
        [Description("Ai Practice Gym")]
        AiPracticeGym,

        [Description("Class Forum Speaking")]
        ClassForumSpeaking,

        [Description("Class Forum Writing")]
        ClassForumWriting,

        [Description("Video Lesson")]
        VideoLesson,

        [Description("Home Work")]
        HomeWork,

        [Description("Test Config Speaking Layout")]
        TestConfigSpeakingLayout,

        [Description("Test Config Writing Layout")]
        TestConfigWritingLayout,

        [Description("Short Answer (Answer base)")]
        ShortAnswerBase,

        [Description("Short Answer (Word count)")]
        ShortAnswerWordCount,

        [Description("Long Answer")]
        LongAnswer,

        ClassForumDefault
    }
}
