// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.AiCriteriaConfig
{
    using Enums;

    public record FeatureTypeMap(EnumFeatureMultiple Feature, List<EnumSubFeatureType> SubFeatures);
    public record FeatureCriteriaMap(EnumSubFeatureType SubFeature, List<EnumCriteriaAi> Criterias);
    public static class FeatureModel
    {
        public static readonly List<FeatureTypeMap> FeatureTypes = new()
        {
            new(EnumFeatureMultiple.Unit,   new() { EnumSubFeatureType.AiPracticeGym }),
            new(EnumFeatureMultiple.Lesson, new() { EnumSubFeatureType.ClassForumSpeaking, EnumSubFeatureType.ClassForumWriting }),
            new(EnumFeatureMultiple.Test,   new() { EnumSubFeatureType.TestConfigSpeakingLayout, EnumSubFeatureType.TestConfigWritingLayout }),
        };

        public static readonly List<FeatureCriteriaMap> FeatureCriteria = new()
        {
            new(EnumSubFeatureType.AiPracticeGym,           new()),
            new(EnumSubFeatureType.ClassForumSpeaking,      new()),
            new(EnumSubFeatureType.ClassForumWriting,       new()),
            new(EnumSubFeatureType.VideoLesson,             new()),
            new(EnumSubFeatureType.HomeWork,                new()),
            new(EnumSubFeatureType.TestConfigSpeakingLayout,new() { EnumCriteriaAi.Fc, EnumCriteriaAi.Lr, EnumCriteriaAi.Gra }),
            new(EnumSubFeatureType.TestConfigWritingLayout, new() { EnumCriteriaAi.Lr, EnumCriteriaAi.Ta, EnumCriteriaAi.Gra, EnumCriteriaAi.Cc }),
        };
    }
}
