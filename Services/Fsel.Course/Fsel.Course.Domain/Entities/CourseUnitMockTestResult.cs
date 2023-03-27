// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using Fsel.Common.Enums.ErrorCodes;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Enums;

    public class CourseUnitMockTestResult : Entity
    {
        public long? Result { get; set; }

        public EnumResultStatus Status { get; set; }

        public CourseUnitMockTest? CourseUnitMockTest { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid CourseUnitMockTestId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid StudentId { get; set; }
    }
}
