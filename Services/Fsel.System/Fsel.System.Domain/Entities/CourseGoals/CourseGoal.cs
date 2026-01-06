// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities.CourseGoals
{
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Enums;

    public class CourseGoal : Entity
    {
        public string? Name { get; set; }
        public EnumCourseGoalCategory GoalCategory { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public EnumCourseType CourseType { get; set; }
        public Guid LevelId { get; set; }
        public Guid? SchoolId { get; set; }
        public string? SchoolName { get; set; }
        public Guid? ClassId { get; set; }
        public string? ClassName { get; set; }
        public ICollection<CourseGoalConfig> CourseGoalConfigs { get; set; } = new List<CourseGoalConfig>();
    }
}
