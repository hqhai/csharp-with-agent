// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.TestModels
{
    using Fsel.Course.Domain.Models.CommandModels.AiGradeSetting;
    using Fsel.Shared.Enums;

    public class TestAICriteriaSettingModel
    {
        public Guid Id { get; set; }
        public string? UserRoleStr { get; set; }
        public IList<TestPromptModel>? AIConfigs { get; set; }
        public string? JsonSchemaStr { get; set; }
        public EnumMockTestAIType CriteriaName { get; set; }
    }
}
