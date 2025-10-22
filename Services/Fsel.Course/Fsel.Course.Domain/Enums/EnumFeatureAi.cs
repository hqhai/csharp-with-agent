// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums
{
    using System.ComponentModel.DataAnnotations;

    public enum EnumFeatureAi
    {
        [Display(Name = "Class Forum - Speaking Layout", GroupName = "Class Forum", Order = 10)]
        ClassForumSpeakingLayout = 101,

        [Display(Name = "Class Forum - Writing Layout", GroupName = "Class Forum", Order = 11)]
        ClassForumWritingLayout = 102,

        [Display(Name = "Test AiConfigSetting - Speaking Layout", GroupName = "Test AiConfigSetting", Order = 20)]
        TestConfigSpeakingLayout = 201,

        [Display(Name = "Test AiConfigSetting - Writing Layout", GroupName = "Test AiConfigSetting", Order = 21)]
        TestConfigWritingLayout = 202,

        [Display(Name = "AI practice Gym", GroupName = "AI Practice", Order = 30)]
        AiPracticeGym = 301,

        [Display(Name = "Short Answer (Answer base)", GroupName = "Short Answer", Order = 40)]
        ShortAnswerBase = 401,

        [Display(Name = "Short Answer (Wordcount)", GroupName = "Short Answer", Order = 41)]
        ShortAnswerWordCount = 402,
    }
}
