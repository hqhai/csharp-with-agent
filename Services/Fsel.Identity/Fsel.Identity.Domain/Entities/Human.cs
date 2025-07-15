// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Attributes;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class Human : Entity
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(100, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? FullName { get; set; }

        [MaxLength(100, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Code { get; set; }

        public DateTime? Birthday { get; set; }

        [PhoneValid(ErrorMessage = nameof(EnumSystemErrorCode.InValidFormat))]
        [MaxLength(20, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? PhoneNumber { get; set; }

        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Address { get; set; }

        [EmailValid(ErrorMessage = nameof(EnumSystemErrorCode.InValidFormat))]
        [MaxLength(70, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Email { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? AvatarPath { get; set; }

        public EnumGender? Gender { get; set; }

        public Guid? ManageUserId { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Position { get; set; }

        public virtual User? User { get; set; }
        public Guid? UserId { get; set; }

        public Parent? Parent { get; set; }
        public CSO? CSO { get; set; }
        public Student? Student { get; set; }
        public Teacher? Teacher { get; set; }
    }
}
