// Copyright (c) Atlantic. All rights reserved.


namespace Fsel.Course.Domain.Models.CommandModels.AiGradeSetting
{
    public class MockTestAiSettingModel
    {
        public IList<SectionAiSettingModel>? MockTestAiSettingModels { get; set; }
    }

    public class SectionAiSettingModel
    {
        public string? SystemRoleAlConfig { get; set; }

        public string? UserAlConfig { get; set; }

        public string? SettingModel { get; set; }

        public double SettingTemperature { get; set; }

        public double SettingWordMaxLength { get; set; }

        public double SettingTopP { get; set; }

        public double SettingFrequecy { get; set; }

        public double SettingPresence { get; set; }

        public string? Task { get; set; }

        public Guid ObjectId { get; set; }

        public IList<MockTestPromptModel>? Prompts { get; set; }
    }
}
