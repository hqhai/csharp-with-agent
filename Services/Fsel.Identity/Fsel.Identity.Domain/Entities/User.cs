// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Fsel.Common.Attributes;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;
using Fsel.Shared.Enums;
using Microsoft.AspNetCore.Identity;

namespace Fsel.Identity.Domain.Entities
{
    public class User : UserEntity
    {
        [Required]
        [MaxLength(100, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? FullName { get; set; }

        [EmailValid(ErrorMessage = nameof(EnumSystemErrorCode.InValidFormat))]
        [MaxLength(70, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        [ProtectedPersonalData]
        public override string? Email { get; set; }

        [PhoneValid(ErrorMessage = nameof(EnumSystemErrorCode.InValidFormat))]
        [MaxLength(20, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        [ProtectedPersonalData]
        public override string? PhoneNumber { get; set; }

        [MaxLength(20, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? DefaultPassword { get; set; }

        public EnumUserStatus? Status { get; set; } = EnumUserStatus.Active;

        public virtual Human? Human { get; set; }

        public virtual ICollection<UserOtpCode> UserOtpCodes { get; set; } = new List<UserOtpCode>();
        public virtual ICollection<UserSetting> UserSettings { get; set; } = new List<UserSetting>();
        public virtual ICollection<UserCourseSetting> UserCourseSettings { get; set; } = new List<UserCourseSetting>();
        public virtual ICollection<UserPlatform> UserPlatforms { get; set; } = new List<UserPlatform>();
        public virtual ICollection<UserReferral> Senders { get; set; } = new List<UserReferral>();
        public virtual ICollection<UserSchool> UserSchools { get; set; } = new List<UserSchool>();
        public virtual UserReferral? Receiver { get; set; }
        public virtual ICollection<UserDeletion> UserDeletions { get; set; } = new List<UserDeletion>();
        public virtual ICollection<UserGroupMemberShip> UserGroups { get; set; } = new List<UserGroupMemberShip>();
    }
}
