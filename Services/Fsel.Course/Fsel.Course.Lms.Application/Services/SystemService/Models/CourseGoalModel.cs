// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService.Models
{
    using System.Collections.Generic;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class CourseGoalModel : BaseModel
    {
        public string? Name { get; set; }
        public EnumCourseGoalCategory GoalCategory { get; set; }
        public Guid? ProgramId { get; set; }
        public Guid? LevelId { get; set; }
        public string? SchoolName { get; set; }
        public Guid? SchoolId { get; set; }
        public string? ClassName { get; set; }
        public Guid? ClassId { get; set; }
        public IList<CourseGoalConfigModel> CourseGoalConfigs { get; set; } = new List<CourseGoalConfigModel>();
    }
}
