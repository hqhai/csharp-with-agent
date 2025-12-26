// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.V1i2
{
    using Entities.TestConfigs;
    using Shared.Enums;
    using SkillModels;
    using TestModels;

    public class TestSectionModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public int DisplayOrder { get; set; }
        public EnumTestLayoutType? LayoutType { get; set; }
        public IList<ScoringFormulaConfig>? ScoringFormulaConfigs { get; set; }
        public double? Percent { get; set; }
        public TestSectionConfig? Config { get; set; }
        public SkillModel? Skill { get; set; }
        public Guid? SkillId { get; set; }
        public IList<TestAISettingModel> TestAISettings { get; set; } = new List<TestAISettingModel>();
        public IList<TestSectionModel> Childrens { get; set; } = new List<TestSectionModel>();
        public IList<Guid> Questions { get; set; } = new List<Guid>();
    }
}
