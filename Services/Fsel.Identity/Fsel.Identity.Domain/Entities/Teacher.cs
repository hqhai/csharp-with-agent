// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Core.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.Enums.ErrorCodes;

    public class Teacher : Entity
    {
        [MaxLength(1000, ErrorMessage = nameof(EnumTeacherErrorCode.TE01C))]
        public string? PassportPath { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumTeacherErrorCode.TE02C))]
        public string? UniversityDegreePath { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumTeacherErrorCode.TE03C))]
        public string? CertificationPath { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumTeacherErrorCode.TE04C))]
        public string? PoliceClearancePath { get; set; }

        public Human? Human { get; set; }

        public Guid HumanId { get; set; }
    }
}
