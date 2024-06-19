// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService.Models
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class CourseTimeConfigModel : BaseModel
    {
        public Guid CourseId { get; set; }

        public int DurationMonth { get; set; }

        public int EnrollmentWeek { get; set; }
    }
}
