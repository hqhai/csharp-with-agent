// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.AiCriteriaConfig
{
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public static class FeatureModel
    {
        public static readonly Dictionary<EnumFeatureMultiple, EnumSubFeatureType[]> FeatureTypes = new()
        {
            {EnumFeatureMultiple.Unit, new [] { EnumSubFeatureType.AiPracticeGym }},
            {EnumFeatureMultiple.Lesson, new [] { EnumSubFeatureType.ClassForumSpeaking, EnumSubFeatureType.ClassForumWriting }},
            {EnumFeatureMultiple.Test,  new [] { EnumSubFeatureType.TestConfigSpeakingLayout , EnumSubFeatureType.TestConfigWritingLayout}},
        };

        public static readonly Dictionary<EnumSubFeatureType, EnumCriteriaAi[]> FeatureCriteria = new()
        {
            {EnumSubFeatureType.AiPracticeGym, Array.Empty<EnumCriteriaAi>()},
            {EnumSubFeatureType.ClassForumSpeaking, Array.Empty<EnumCriteriaAi>()},
            {EnumSubFeatureType.ClassForumWriting, Array.Empty<EnumCriteriaAi>()},
            {EnumSubFeatureType.VideoLesson, Array.Empty<EnumCriteriaAi>()},
            {EnumSubFeatureType.HomeWork, Array.Empty<EnumCriteriaAi>()},
            {EnumSubFeatureType.TestConfigSpeakingLayout, new [] { EnumCriteriaAi.Fc, EnumCriteriaAi.Lr, EnumCriteriaAi.Gra }},
            {EnumSubFeatureType.TestConfigWritingLayout, new [] { EnumCriteriaAi.Lr, EnumCriteriaAi.Ta, EnumCriteriaAi.Gra, EnumCriteriaAi.Cc }},

        };
    }
}
