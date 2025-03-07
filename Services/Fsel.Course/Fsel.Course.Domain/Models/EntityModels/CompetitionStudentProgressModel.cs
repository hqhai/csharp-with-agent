// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class CompetitionStudentProgressModel
    {
        public Guid CourseId { get; set; }
        public string? CourseName { get; set; }
        public double ContentCompleted { get; set; }
        public Guid StudentId { get; set; }
        public double TotalScore { get; set; }
        public Guid CourseResultId { get; set; }
        public EnumCourseType CourseType { get; set; }
    }
}
