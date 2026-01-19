// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.AiCriteriaConfig
{
    using System.Collections.Immutable;
    using Enums;

    public record FeatureTypeMap(EnumFeatureMultiple Feature, ImmutableList<EnumSubFeatureType> SubFeatures);
    public record FeatureCriteriaMap(EnumSubFeatureType SubFeature, ImmutableList<EnumCriteriaAi> Criterias);

    public static class FeatureModel
    {
        private static readonly IReadOnlyDictionary<EnumFeatureMultiple, IReadOnlyList<EnumSubFeatureType>> _featureSubFeatures;
        private static readonly IReadOnlyDictionary<EnumSubFeatureType, IReadOnlyList<EnumCriteriaAi>> _subFeatureCriteria;

        static FeatureModel()
        {
            _featureSubFeatures = FeatureTypes
                .ToImmutableDictionary(x => x.Feature, x => (IReadOnlyList<EnumSubFeatureType>)x.SubFeatures);

            _subFeatureCriteria = FeatureCriteria
                .ToImmutableDictionary(x => x.SubFeature, x => (IReadOnlyList<EnumCriteriaAi>)x.Criterias);
        }

        public static IReadOnlyDictionary<EnumFeatureMultiple, IReadOnlyList<EnumSubFeatureType>> FeatureSubFeatures => _featureSubFeatures;
        public static IReadOnlyDictionary<EnumSubFeatureType, IReadOnlyList<EnumCriteriaAi>> SubFeatureCriteria => _subFeatureCriteria;

        private static readonly ImmutableList<FeatureTypeMap> _featureTypes = ImmutableList.Create(
            new FeatureTypeMap(EnumFeatureMultiple.Unit, ImmutableList.Create(EnumSubFeatureType.AiPracticeGym)),
            new FeatureTypeMap(EnumFeatureMultiple.Lesson, ImmutableList.Create(
                EnumSubFeatureType.ClassForumSpeaking,
                EnumSubFeatureType.ClassForumWriting,
                EnumSubFeatureType.VideoLesson,
                EnumSubFeatureType.HomeWork
            )),
            new FeatureTypeMap(EnumFeatureMultiple.Test, ImmutableList.Create(
                EnumSubFeatureType.TestConfigSpeakingLayout,
                EnumSubFeatureType.TestConfigWritingLayout,
                EnumSubFeatureType.ShortAnswerBase
            ))
        );

        private static readonly ImmutableList<FeatureCriteriaMap> _featureCriteria = ImmutableList.Create(
            new FeatureCriteriaMap(EnumSubFeatureType.AiPracticeGym, ImmutableList<EnumCriteriaAi>.Empty),
            new FeatureCriteriaMap(EnumSubFeatureType.ClassForumSpeaking, ImmutableList<EnumCriteriaAi>.Empty),
            new FeatureCriteriaMap(EnumSubFeatureType.ClassForumWriting, ImmutableList<EnumCriteriaAi>.Empty),
            new FeatureCriteriaMap(EnumSubFeatureType.VideoLesson, ImmutableList<EnumCriteriaAi>.Empty),
            new FeatureCriteriaMap(EnumSubFeatureType.HomeWork, ImmutableList<EnumCriteriaAi>.Empty),
            new FeatureCriteriaMap(EnumSubFeatureType.TestConfigSpeakingLayout, ImmutableList.Create(
                EnumCriteriaAi.Fc, EnumCriteriaAi.Lr, EnumCriteriaAi.Gra
            )),
            new FeatureCriteriaMap(EnumSubFeatureType.TestConfigWritingLayout, ImmutableList.Create(
                EnumCriteriaAi.Lr, EnumCriteriaAi.Ta, EnumCriteriaAi.Gra, EnumCriteriaAi.Cc
            ))
        );

        public static IReadOnlyList<FeatureTypeMap> FeatureTypes => _featureTypes;
        public static IReadOnlyList<FeatureCriteriaMap> FeatureCriteria => _featureCriteria;
    }
}
