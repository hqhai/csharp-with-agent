// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;

namespace Fsel.Identity.Domain.Entities
{
    public class User : UserEntity
    {
        [Required]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? FullName { get; set; }

        public virtual Human? Human { get; set; }

        public virtual ICollection<UserOtpCode> UserOtpCodes { get; set; } = new List<UserOtpCode>();
        public virtual ICollection<UserSetting> UserSettings { get; set; } = new List<UserSetting>();
        public virtual ICollection<UserCourseSetting> UserCourseSettings { get; set; } = new List<UserCourseSetting>();
        public virtual ICollection<UserPlatform> UserPlatforms { get; set; } = new List<UserPlatform>();
        public virtual ICollection<UserReferral> Senders { get; set; } = new List<UserReferral>();
        //public virtual ICollection<UserRoleEntity> UserRoles { get; set; } = new List<UserRoleEntity>();
        public virtual UserReferral? Receiver { get; set; }
    }
}
