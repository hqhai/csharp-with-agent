// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.TestConfigSections
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Models.CommandModels.Questions;
    using Fsel.Shared.Enums;

    public class UpdateTestConfigSectionCommandModel : BaseCommandModel
    {
        public string? Name { get; set; }
        public string? ConfigStr { get; set; }
        public int DisplayOrder { get; set; }
        public double? ExecutionTime { get; set; }
        public double? TotalScore { get; set; }
        public int TargetWord { get; set; }
        public EnumTestLayoutType LayoutType { get; set; }
        public Guid? TestConfigId { get; set; }
        public Guid? SkillId { get; set; }

        public IList<UpdateQuestionCommandModel>? Questions { get; set; }
        public IList<CreateTestConfigSectionCommandModel>? Childrens { get; set; }
    }
}
