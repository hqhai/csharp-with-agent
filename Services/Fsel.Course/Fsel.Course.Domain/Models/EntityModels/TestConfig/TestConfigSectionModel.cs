// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.TestConfig
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.Questions;
    using Fsel.Shared.Enums;

    public class TestConfigSectionModel : BaseModel
    {
        public string? Name { get; set; }
        public int TargetWord { get; set; }
        public int DisplayOrder { get; set; }
        public double? ExecutionTime { get; set; }
        public EnumTestLayoutType LayoutType { get; set; }
        public double? TotalScore { get; set; }
        public object? Config { get; set; }
        public string? ConfigStr { get; set; }
        public Guid? ParentId { get; set; }
        public TestConfigSectionModel? Parent { get; set; }
        public Guid? TestConfigId { get; set; }
        public Guid? SkillId { get; set; }
        public Skill? Skill { get; set; }
        public IList<TestConfigSectionModel>? Children { get; set; }
        public IList<QuestionModel>? Questions { get; set; }
    }
}
