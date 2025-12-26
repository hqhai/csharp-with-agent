// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.TestModels
{
    using System;
    using System.Collections.Generic;
    using Fsel.Course.Domain.Models.CommandModels.AiGradeSetting;

    public class TestAISettingModel
    {
        public Guid Id { get; set; }
        public string? SettingModel { get; set; }
        public double SettingTemperature { get; set; }
        public double SettingWordMaxLength { get; set; }
        public double SettingTopP { get; set; }
        public double SettingFrequecy { get; set; }
        public double SettingPresence { get; set; }
        public string? SystemRoleAlConfig { get; set; }
        public string? UserAlConfig { get; set; }
        public string? Task { get; set; }
        public IList<TestPromptModel> Prompts { get; set; } = new List<TestPromptModel>();
        public IList<TestAICriteriaSettingModel> TestAICriteriaSettings { get; set; } = new List<TestAICriteriaSettingModel>();
    }
}
