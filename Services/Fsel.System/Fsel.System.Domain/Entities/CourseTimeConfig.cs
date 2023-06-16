// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using global::System.ComponentModel.DataAnnotations;

    public class CourseTimeConfig : Entity
    {
        public EnumCourseLevel CourseLevel { get; set; }

        [Range(0, 1000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int DurationMonth { get; set; }

        [Range(0, 1000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int EnrollmentWeek { get; set; }
    }
}
