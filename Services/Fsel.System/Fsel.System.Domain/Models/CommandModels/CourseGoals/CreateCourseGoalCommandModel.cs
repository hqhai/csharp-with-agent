// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.CourseGoals
{
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Enums;

    public class CreateCourseGoalCommandModel
    {
        public EnumCourseGoalCategory GoalCategory { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public EnumCourseType CourseType { get; set; }
        public Guid? SchoolId { get; set; }
        public string? SchoolName { get; set; }
        public IList<SchoolClassModel>? Classes { get; set; }
        public IList<CreateCourseGoalConfigCommandModel> CourseGoalConfigs { get; set; } = new List<CreateCourseGoalConfigCommandModel>();
    }

    public class SchoolClassModel
    {
        public Guid ClassId { get; set; }
        public string? Name { get; set; }
    }

    public class CreateCourseGoalConfigCommandModel
    {
        public int LessonsPerWeek { get; set; }
        public Guid CourseId { get; set; }
    }
}
