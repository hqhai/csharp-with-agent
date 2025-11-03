// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class CourseGoalConfigModel : BaseModel
    {
        public int LessonsPerWeek { get; set; }
        public Guid CourseId { get; set; }
        public int DisplayOrder { get; set; }
        public Guid CourseGoalId { get; set; }
        public string? CourseName { get; set; }
    }
}
