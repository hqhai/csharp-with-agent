// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.TestSections
{
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Models.CommandModels.Questions;
    using Fsel.Course.Domain.Models.CommandModels.TestAiSettings;
    using Fsel.Shared.Enums;

    public class UpdateTestSectionCommandModel
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public TestSectionConfig? Config { get; set; }
        public EnumTestLayoutType? LayoutType { get; set; }
        public Guid? SkillId { get; set; }
        public IList<ScoringFormulaConfig>? ScoringFormulaConfigs { get; set; }
        public string? I18nSkillDescription { get; set; }
        public IList<ReportContentBankConfig>? ReportContentBankConfigs { get; set; }
        public double? Percent { get; set; }
        public Guid? AiPromptManagerId { get; set; }
        public IList<UpdateTestAISettingCommandModel> TestAISettings { get; set; } = new List<UpdateTestAISettingCommandModel>();
        public IList<UpdateQuestionCommandModel> Questions { get; set; } = new List<UpdateQuestionCommandModel>();
        public IList<UpdateTestSectionCommandModel> Childrens { get; set; } = new List<UpdateTestSectionCommandModel>();
    }
}
