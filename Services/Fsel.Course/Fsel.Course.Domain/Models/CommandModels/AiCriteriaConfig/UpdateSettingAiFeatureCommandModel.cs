// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.AiCriteriaConfig
{
    using Enums;

    public class UpdateSettingAiFeatureCommandModel
    {
        public Guid ProjectId { get; set; }
        public Guid? ObjectId { get; set; }
        public EnumSubFeatureType SubFeatureType { get; set; }
        public double SettingTemperature { get; set; }
        public double SettingWordMaxLength { get; set; }
        public double SettingTopP { get; set; }
        public double SettingFrequency { get; set; }
        public double SettingPresence { get; set; }
        public int? MaximumNumber { get; set; }
        public int? MaximumToken { get; set; }
    }
}
