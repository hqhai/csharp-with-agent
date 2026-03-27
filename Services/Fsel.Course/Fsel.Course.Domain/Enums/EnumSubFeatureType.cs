// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public enum EnumSubFeatureType
    {
        [Description("Ai Practice Gym"), Display(Name = "Ai Practice Gym")]
        AiPracticeGym,

        [Description("Class Forum Speaking"), Display(Name = "Class Forum Speaking")]
        ClassForumSpeaking,

        [Description("Class Forum Writing"), Display(Name = "Class Forum Writing")]
        ClassForumWriting,

        [Description("Video Lesson"), Display(Name = "Video Lesson")]
        VideoLesson,

        [Description("Homework"), Display(Name = "Homework")]
        Homework,

        [Description("Test Config Speaking Layout"), Display(Name = "Test Config Speaking Layout")]
        TestConfigSpeakingLayout,

        [Description("Test Config Writing Layout"), Display(Name = "Test Config Writing Layout")]
        TestConfigWritingLayout,

        [Description("Short Answer (Answer base)"), Display(Name = "Short Answer (Answer base)")]
        ShortAnswerBase,

        [Description("Short Answer (Word count)"), Display(Name = "Short Answer (Word count)")]
        ShortAnswerWordCount,

        [Description("Long Answer"), Display(Name = "Long Answer")]
        LongAnswer,

        [Description("Class Forum Default"), Display(Name = "Class Forum Default")]
        ClassForumDefault,

        [Description("AI Response Translation")]
        AiResponseTranslation,

        [Description("Ai Practice Gym Reading")]
        AiPracticeGymReading,

        [Description("Ai Practice Gym Listening")]
        AiPracticeGymListening,

        [Description("Ai Practice Gym Writing")]
        AiPracticeGymWriting,

        [Description("Ai Practice Gym Speaking")]
        AiPracticeGymSpeaking,

        [Description("Ai Practice Gym Vocabulary")]
        AiPracticeGymVocabulary,

        [Description("Ai Practice Gym Grammar")]
        AiPracticeGymGrammar,

        [Description("Vstep")]
        Vstep,

        [Description("Video Lesson Long Answer"), Display(Name = "Video Lesson Long Answer")]
        VideoLessonLongAnswer,

        [Description("HomeWork Long Answer"), Display(Name = "HomeWork Long Answer")]
        HomeWorkLongAnswer,
    }
}
