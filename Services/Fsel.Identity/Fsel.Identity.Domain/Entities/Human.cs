// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Core.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.Enums.ErrorCodes;

    public class Human : Entity
    {
        [Required(ErrorMessage = nameof(EnumHumanErrorCode.HM01C))]
        [MaxLength(250, ErrorMessage = nameof(EnumHumanErrorCode.HM02C))]
        public string? FullName { get; set; }

        public DateTime? Birthday { get; set; }

        public string? PhoneNumber { get; set; }

        [MaxLength(250, ErrorMessage = nameof(EnumHumanErrorCode.HM02C))]
        public string? Address { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumHumanErrorCode.HM03C))]
        public string? AvatarPath { get; set; }

        public EnumGender? Gender { get; set; }

        public virtual User? User { get; set; }
        public string? UserId { get; set; }

        public Parent? Parent { get; set; }
        public Student? Student { get; set; }
        public Teacher? Teacher { get; set; }
    }
}
