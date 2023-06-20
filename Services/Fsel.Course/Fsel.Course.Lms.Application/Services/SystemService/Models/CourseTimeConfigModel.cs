// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService.Models
{
    using System;

    public class CourseTimeConfigModel
    {
        public Guid CourseId { get; set; }

        public int DurationMonth { get; set; }

        public int EnrollmentWeek { get; set; }
    }
}
