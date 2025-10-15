// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Enums;

    public class CourseGoalModel : BaseModel
    {
        public string? Name { get; set; }
        public EnumCourseGoalCategory GoalCategory { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public EnumCourseType CourseType { get; set; }
        public string? SchoolName { get; set; }
        public string? ClassName { get; set; }
        public IList<CourseGoalConfigModel> CourseGoalConfigs { get; set; } = new List<CourseGoalConfigModel>();
    }
}
