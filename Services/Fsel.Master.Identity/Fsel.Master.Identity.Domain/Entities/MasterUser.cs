// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Fsel.Common.Attributes;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;
using Fsel.Shared.Enums;
using Microsoft.AspNetCore.Identity;

namespace Fsel.Master.Identity.Domain.Entities
{
    public class MasterUser : UserEntity
    {
        [Key]
        [Column(Order = 0)]
        public override Guid Id { get; set; }

        [EmailValid(ErrorMessage = nameof(EnumSystemErrorCode.InValidFormat))]
        [MaxLength(70, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        [ProtectedPersonalData]
        public override string? Email { get; set; }

        [PhoneValid(ErrorMessage = nameof(EnumSystemErrorCode.InValidFormat))]
        [MaxLength(20, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        [ProtectedPersonalData]
        public override string? PhoneNumber { get; set; }

        [MaxLength(250)]
        public string? Code { get; set; }

        [MaxLength(250)]
        [Required]
        public string? FirstName { get; set; }

        [MaxLength(250)]
        public string? LastName { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string? FullName { get; private set; }

        public DateTime? Birthday { get; set; }

        public EnumGender? Gender { get; set; }

        [MaxLength(250)]
        public string? Address { get; set; }

        [MaxLength(1000)]
        public string? AvatarPath { get; set; }

        [MaxLength(20, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? DefaultPassword { get; set; }

        public EnumUserStatus? Status { get; set; } = EnumUserStatus.Active;
    }
}
