// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.CommandModels.Ais
{
    public class SubmitAICommandModel
    {
        public string? SettingModel { get; set; }

        public double SettingTemperature { get; set; }

        public double SettingWordMaxLength { get; set; }

        public double SettingTopP { get; set; }

        public double SettingFrequecy { get; set; }

        public double SettingPresence { get; set; }

        public string? SystemRoleAlConfig { get; set; }

        public string? UserAIConfig { get; set; }

        public object? Format { get; set; }
    }
}
