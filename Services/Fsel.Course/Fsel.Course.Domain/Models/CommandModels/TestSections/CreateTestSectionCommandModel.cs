// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.TestSections
{
    using Fsel.Course.Domain.Models.CommandModels.Questions;
    using Fsel.Shared.Enums;

    public class CreateTestSectionCommandModel
    {
        public string? Name { get; set; }
        public object? Config { get; set; }
        public int DisplayOrder { get; set; }
        public double? ExecutionTime { get; set; }
        public double? TotalScore { get; set; }
        public int TargetWord { get; set; }
        public EnumTestLayoutType LayoutType { get; set; }
        public Guid? ParentId { get; set; }
        public Guid? TestConfigId { get; set; }
        public Guid? SkillId { get; set; }

        public IList<CreateQuestionCommandModel> Questions { get; set; } = new List<CreateQuestionCommandModel>();
        public IList<CreateTestSectionCommandModel> Childrens { get; set; } = new List<CreateTestSectionCommandModel>();
    }
}
