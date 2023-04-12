// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Shared.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;

    public class Student : Entity
    {
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Membership { get; set; }

        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Occupation { get; set; }

        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? School { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public Guid ClassId { get; set; }

        public Human? Human { get; set; }

        public Guid HumanId { get; set; }

        public IList<ParentStudent> ParentStudents { get; set; } = new List<ParentStudent>();
    }
}
