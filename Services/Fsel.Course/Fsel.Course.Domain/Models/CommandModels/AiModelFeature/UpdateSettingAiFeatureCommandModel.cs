// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.AiModelFeature
{
    using Fsel.Shared.Enums;

    public class UpdateSettingAiFeatureCommandModel
    {
        public EnumFeature Key { get; set; }
        public double SettingTemperature { get; set; }
        public double SettingWordMaxLength { get; set; }
        public double SettingTopP { get; set; }
        public double SettingFrequency { get; set; }
        public double SettingPresence { get; set; }
        public int? MaximumNumber { get; set; }
        public int? MaximumToken { get; set; }
    }
}
