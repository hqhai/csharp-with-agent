// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class CourseCompleteModel
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
        public int CountComplete { get; set; }
        public int TotalComplete { get; set; }
        public int? UnitDisplayOrder { get; set; } = 1;
        public int? LessonDisplayOrder { get; set; } = 1;
    }
}
