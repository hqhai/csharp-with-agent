// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Fsel.Common.Attributes;
using Fsel.Common.Enums.ErrorCodes;
using System.ComponentModel.DataAnnotations.Schema;
using Fsel.Core.Entities;
using Fsel.Shared.Enums;
using Microsoft.AspNetCore.Identity;

namespace Fsel.Identity.Domain.Entities
{
    public class User : UserEntity
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
        [Required]
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

        public Guid? ManageUserId { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Position { get; set; }


        public virtual ICollection<UserOtpCode> UserOtpCodes { get; set; } = new List<UserOtpCode>();

        public virtual ICollection<UserSetting> UserSettings { get; set; } = new List<UserSetting>();

        public virtual ICollection<UserCourseSetting> UserCourseSettings { get; set; } = new List<UserCourseSetting>();

        public virtual ICollection<UserPlatform> UserPlatforms { get; set; } = new List<UserPlatform>();

        public virtual ICollection<UserReferral> Senders { get; set; } = new List<UserReferral>();

        public virtual ICollection<UserSchool> UserSchools { get; set; } = new List<UserSchool>();

        public virtual UserReferral? Receiver { get; set; }

        public virtual ICollection<UserDeletion> UserDeletions { get; set; } = new List<UserDeletion>();

        public virtual Parent? Parent { get; set; }
        public virtual CSO? CSO { get; set; }
        public virtual Student? Student { get; set; }
        public virtual Teacher? Teacher { get; set; }
    }
}
