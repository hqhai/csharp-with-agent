// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.TestAiSettings
{
    using System.Collections.Generic;
    using Fsel.Course.Domain.Models.CommandModels.AiGradeSetting;
    using Fsel.Shared.Enums;

    public class UpdateTestAISettingCommandModel
    {
        public Guid? Id { get; set; }
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
        public IList<UpdateTestAICriteriaSettingCommandModel> TestAICriteriaSettings { get; set; } = new List<UpdateTestAICriteriaSettingCommandModel>();
    }

    public class UpdateTestAICriteriaSettingCommandModel
    {
        public Guid? Id { get; set; }
        public string? UserRoleStr { get; set; }
        public string? JsonSchemaStr { get; set; }
        public IList<TestPromptModel> AIConfigs { get; set; } = new List<TestPromptModel>();
        public EnumMockTestAIType CriteriaName { get; set; }
    }
}
