// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.TestModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Models.EntityModels.SkillModels;
    using Fsel.Shared.Enums;

    public class TestSectionModel : BaseModel
    {
        public string? Name { get; set; }
        public int DisplayOrder { get; set; }
        public EnumTestLayoutType LayoutType { get; set; }
        public TestSectionConfig? Config { get; set; }
        public SkillModel? Skill { get; set; }
        public IList<TestSectionModel> Childrens { get; set; } = new List<TestSectionModel>();
        public IList<QuestionModel> Questions { get; set; } = new List<QuestionModel>();
    }
}
