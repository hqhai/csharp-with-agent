// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.GameTopics
{
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Enums;

    public class SaveGameTopicCommandModel
    {
        public Guid? Id { get; set; }
        public EnumUnitOrder UnitOrder { get; set; }

        public string? Value { get; set; }

        public EnumCourseSkill Skill { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }
    }
}
