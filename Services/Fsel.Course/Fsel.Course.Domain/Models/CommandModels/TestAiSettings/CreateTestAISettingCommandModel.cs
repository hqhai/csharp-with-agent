// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.TestAiSettings
{
    using Fsel.Course.Domain.Models.CommandModels.AiGradeSetting;
    using Fsel.Shared.Enums;

    public class CreateTestAISettingCommandModel
    {
        public bool IsUseAIGrade { get; set; }
        public string? SystemRoleAlConfig { get; set; }
        public string? UserAlConfig { get; set; }
        public string? SettingModel { get; set; }
        public double? SettingTemperature { get; set; }
        public double? SettingWordMaxLength { get; set; }
        public double? SettingTopP { get; set; }
        public double? SettingFrequecy { get; set; }
        public double? SettingPresence { get; set; }
        public string? Task { get; set; }
        public IList<TestPromptModel> Prompts { get; set; } = new List<TestPromptModel>();
        public IList<CreateTestAICriteriaSettingCommandModel> TestAICriteriaSettings { get; set; } = new List<CreateTestAICriteriaSettingCommandModel>();
    }

    public class CreateTestAICriteriaSettingCommandModel
    {
        public string? UserRoleStr { get; set; }
        public string? JsonSchemaStr { get; set; }
        public EnumMockTestAIType CriteriaName { get; set; }
        public IList<TestPromptModel> AIConfigs { get; set; } = new List<TestPromptModel>();
    }
}
