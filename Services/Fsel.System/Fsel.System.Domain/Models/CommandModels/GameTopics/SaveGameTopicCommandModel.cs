// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.GameTopics
{
    using Fsel.Shared.Enums;

    public class SaveGameTopicCommandModel
    {
        public Guid? Id { get; set; }
        public EnumUnitNumber UnitOrder { get; set; }
        public string? Value { get; set; }
        public EnumCourseSkill Skill { get; set; }
        public string? SkillName { get; set; }
        public Guid? SkillId { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
    }
}
