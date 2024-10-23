// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.LmsCourseService.Model
{
    public class CompetitionStudentProgressModel
    {
        public Guid CourseId { get; set; }
        public string? CourseName { get; set; }
        public double ContentCompleted { get; set; }
        public Guid StudentId { get; set; }
        public double TotalScore { get; set; }

        public Guid CourseResultId { get; set; }

    }
}
