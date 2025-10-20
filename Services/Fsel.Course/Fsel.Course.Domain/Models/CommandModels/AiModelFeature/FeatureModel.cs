// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.AiModelFeature
{
    using Fsel.Course.Domain.Enums;

    public static class FeatureModel
    {
        public static readonly Dictionary<EnumFeatureAi, EnumTypeFeatureAi[]> FeatureTypes = new()
        {
            {EnumFeatureAi.TestConfigSpeakingLayout, new[]{ EnumTypeFeatureAi.Fc, EnumTypeFeatureAi.Lr, EnumTypeFeatureAi.Gra}},
            {EnumFeatureAi.TestConfigWritingLayout, new[]{ EnumTypeFeatureAi.Lr, EnumTypeFeatureAi.Ta, EnumTypeFeatureAi.Gra,  EnumTypeFeatureAi.Cc}},
            {EnumFeatureAi.ClassForumWritingLayout,  Array.Empty<EnumTypeFeatureAi>()},
            {EnumFeatureAi.ClassForumSpeakingLayout, Array.Empty<EnumTypeFeatureAi>()},
            {EnumFeatureAi.ShortAnswerBase, Array.Empty < EnumTypeFeatureAi >()},
            {EnumFeatureAi.ShortAnswerWordCount, Array.Empty < EnumTypeFeatureAi >()},
            {EnumFeatureAi.AiPracticeGym, Array.Empty < EnumTypeFeatureAi >()}
        };
    }
}
