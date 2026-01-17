// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums
{
    using System.ComponentModel;

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
        ClassForumDefault,
        [Description("AI Response Translation")]
        AiResponseTranslation
    }
}
