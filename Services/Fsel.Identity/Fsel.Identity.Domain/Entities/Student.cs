// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Common.Enums;
    using Fsel.Core.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;

    public class Student : Entity
    {
        [Required(ErrorMessage = nameof(EnumStudentErrorCode.ST01C))]
        [MaxLength(250, ErrorMessage = nameof(EnumStudentErrorCode.ST02C))]
        public string? Membership { get; set; }

        [MaxLength(250, ErrorMessage = nameof(EnumStudentErrorCode.ST02C))]
        public string? Occupation { get; set; }

        [MaxLength(250, ErrorMessage = nameof(EnumStudentErrorCode.ST02C))]
        public string? School { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public Guid ClassId { get; set; }

        public Human? Human { get; set; }

        public Guid HumanId { get; set; }

        public List<ParentStudent> ParentStudents { get; set; } = new List<ParentStudent>();
    }
}
