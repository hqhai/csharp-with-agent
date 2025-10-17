// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels.IntegrationModel
{
    public class CourseIntegrationModel
    {
        public string? CourseName { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int? TotalLesson { get; set; }

        public int? TotalLessonDone { get; set; }

        public string? NameCurrentUnit { get; set; }

        public string? NameCurrentLesson { get; set; }
    }
}
