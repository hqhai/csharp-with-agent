// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.AiModelFeature
{
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public static class FeatureModel
    {
        public static readonly Dictionary<EnumFeature, EnumTypeFeatureAi[]> FeatureTypes = new()
        {
            {EnumFeature.TestConfigSpeakingLayout, new[]{ EnumTypeFeatureAi.Fc, EnumTypeFeatureAi.Lr, EnumTypeFeatureAi.Gra}},
            {EnumFeature.TestConfigWritingLayout, new[]{ EnumTypeFeatureAi.Lr, EnumTypeFeatureAi.Ta, EnumTypeFeatureAi.Gra,  EnumTypeFeatureAi.Cc}},
            {EnumFeature.ClassForumWritingLayout,  Array.Empty<EnumTypeFeatureAi>()},
            {EnumFeature.ClassForumSpeakingLayout, Array.Empty<EnumTypeFeatureAi>()},
            {EnumFeature.ShortAnswerBase, Array.Empty < EnumTypeFeatureAi >()},
            {EnumFeature.ShortAnswerWordCount, Array.Empty < EnumTypeFeatureAi >()},
            {EnumFeature.AiPracticeGym, Array.Empty < EnumTypeFeatureAi >()}
        };
    }
}
