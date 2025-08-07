// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.IntegrationModels
{
    public class InfoCourseIntegrationModel
    {
        public Guid UserId { get; set; }

        public IList<InfoCourseIntegrationDetailModel>? InfoCourseIntegrationDetails { get; set; }
    }

    public class InfoCourseIntegrationDetailModel
    {
        public Guid CourseId { get; set; }

        public Guid StudentId { get; set; }

        public string? CourseName { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int TotalLesson { get; set; }

        public int TotalLessonDone { get; set; }

        public string? NameCurrentUnit { get; set; }

        public string? NameCurrentLesson { get; set; }
    }
}
