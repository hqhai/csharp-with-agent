// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Services.SystemServices.Model
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
