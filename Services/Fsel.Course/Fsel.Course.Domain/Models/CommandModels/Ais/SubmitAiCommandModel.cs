// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Ais
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

        public object? Text { get; set; }

        public string? NameSchema { get; set; }

        public string? SchemaType { get; set; }
    }
}
