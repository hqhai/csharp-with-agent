// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.AiCriteriaConfig
{
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public static class FeatureModel
    {
        public static readonly Dictionary<EnumFeature, EnumCriteriaAi[]> FeatureTypes = new()
        {
            {EnumFeature.ClassForum, Array.Empty<EnumCriteriaAi>()},
            {EnumFeature.HomeWork, Array.Empty<EnumCriteriaAi>()},
            {EnumFeature.MockTest,  Array.Empty<EnumCriteriaAi>()},
            {EnumFeature.ChatBot, Array.Empty<EnumCriteriaAi>()},
            {EnumFeature.VideoLesson, Array.Empty < EnumCriteriaAi >()},
            {EnumFeature.FinalTest, Array.Empty < EnumCriteriaAi >()},
            {EnumFeature.DiscussionBoard, Array.Empty < EnumCriteriaAi >()}
        };
    }
}
