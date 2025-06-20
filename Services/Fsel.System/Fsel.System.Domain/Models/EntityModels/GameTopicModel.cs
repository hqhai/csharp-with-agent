// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class GameTopicModel : BaseModel
    {
        public EnumUnitNumber UnitOrder { get; set; }

        public string? Value { get; set; }

        public EnumCourseSkill Skill { get; set; }
        public Guid? SkillId { get; set; }
        public string? SkillName { get; set; }
        public EnumGameCourseLevel CourseLevel { get; set; }
    }
}
