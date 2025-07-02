// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;

    public class CourseCompleteModel
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
        public int CountComplete { get; set; }
        public int TotalComplete { get; set; }
        public UnitResultModel? UnitResult { get; set; }
        public LessonResultModel? LessonResult { get; set; }
        public int TotalLessonDone { get; set; }
        public int TotalLesson { get; set; }
        public int? UnitDisplayOrder { get; set; } = 1;
        public int? LessonDisplayOrder { get; set; } = 1;
    }

    public class CourseLessonModel
    {
        public Guid CourseId { get; set; }
        public int TotalLesson { get; set; }
    }
}
