// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.AiManagerModels
{
    public class SetiingAiFeatureModel
    {
        public double? SettingTemperature { get; set; }
        public double? SettingWordMaxLength { get; set; }
        public double? SettingTopP { get; set; }
        public double? SettingFrequency { get; set; }
        public double? SettingPresence { get; set; }
        public int? MaximumNumber { get; set; }
        public int? MaximumToken { get; set; }
        public SetiingAiFeatureModel(double? settingFrequency, double? settingTemperature, double? settingPresence, double? settingTopP, double? settingWordMaxLength, int? maximumNumber, int? maximumToken)
        {
            SettingFrequency = settingFrequency;
            SettingTemperature = settingTemperature;
            SettingPresence = settingPresence;
            SettingTopP = settingTopP;
            SettingWordMaxLength = settingWordMaxLength;
            MaximumNumber = maximumNumber;
            MaximumToken = maximumToken;
        }
    }
}
