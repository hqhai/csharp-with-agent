// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Enums;

    public class GameTopicModel : BaseModel
    {
        public EnumUnitOrder UnitOrder { get; set; }

        public string? Value { get; set; }

        public EnumCourseSkill Skill { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }
    }
}
