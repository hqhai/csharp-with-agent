// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.TestSections
{
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Models.CommandModels.Questions;
    using Fsel.Course.Domain.Models.CommandModels.TestAiSettings;
    using Fsel.Shared.Enums;

    public class CreateTestSectionCommandModel
    {
        public string? Name { get; set; }
        public TestSectionConfig? Config { get; set; }
        public EnumTestLayoutType? LayoutType { get; set; }
        public Guid? SkillId { get; set; }
        public IList<ScoringFormulaConfig>? ScoringFormulaConfigs { get; set; }
        public double? Percent { get; set; }
        public IList<CreateTestAISettingCommandModel> TestAISettings { get; set; } = new List<CreateTestAISettingCommandModel>();
        public IList<CreateQuestionCommandModel> Questions { get; set; } = new List<CreateQuestionCommandModel>();
        public IList<CreateTestSectionCommandModel> Childrens { get; set; } = new List<CreateTestSectionCommandModel>();
    }
}
