// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Sections
{
    using Fsel.Course.Domain.Models.CommandModels.AiGradeSetting;
    using Fsel.Course.Domain.Models.CommandModels.Questions;
    using Fsel.Course.Domain.Models.CommandModels.SectionParts;
    using Fsel.Course.Domain.Models.CommandModels.SectionTimeCodes;
    using Fsel.Shared.Enums;

    public class CreateSectionCommandModel
    {
        public string? Name { get; set; }
        public string? MediaPost { get; set; }
        public int? TargetWord { get; set; }
        public int DisplayOrder { get; set; }
        public string? VideoFilePath { get; set; }
        public string? SubFilePath { get; set; }
        public IList<CreateSectionPartCommandModel>? SectionParts { get; set; }
        public IList<CreateQuestionCommandModel>? Questions { get; set; }
        public IList<CreateSectionTimeCodeCommandModel>? SectionTimeCodes { get; set; }
        public IList<MockTestAISettingModel>? MockTestAISettings { get; set; }
    }

    public class MockTestAISettingModel
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
        public IList<MockTestPromptModel>? Prompts { get; set; }
        public IList<MockTestAICriteriaSettingModel>? MockTestAICriteriaSettings { get; set; }
    }

    public class MockTestAICriteriaSettingModel
    {
        public string? SystemRoleAlConfig { get; set; }
        public string? PromptStr { get; set; }
        public IList<MockTestPromptModel>? Prompts { get; set; }
        public Guid? MockTestAISettingId { get; set; }
        public EnumMockTestAIType CriteriaName { get; set; }
    }
}
