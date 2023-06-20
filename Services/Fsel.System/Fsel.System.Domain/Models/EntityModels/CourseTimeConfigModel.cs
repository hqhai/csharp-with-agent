// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class CourseTimeConfigModel : BaseModel
    {
        public Guid CourseId { get; set; }

        public int DurationMonth { get; set; }

        public int EnrollmentWeek { get; set; }
    }
}
