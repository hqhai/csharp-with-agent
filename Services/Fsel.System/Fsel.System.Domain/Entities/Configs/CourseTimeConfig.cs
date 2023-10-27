// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities.Configs
{
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using global::System.ComponentModel.DataAnnotations;

    public class CourseTimeConfig : Entity
    {
        public Guid CourseId { get; set; }

        [Range(0, 1000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int DurationMonth { get; set; }

        [Range(0, 1000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int EnrollmentWeek { get; set; }
    }
}
