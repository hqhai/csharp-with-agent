// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace Fsel.Identity.Domain.Entities
{
    public class User : IdentityUser, IEntity
    {
        [Required]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? FullName { get; set; }

        public virtual Human? Human { get; set; }

        public virtual ICollection<UserOtpCode> UserOtpCodes { get; set; } = new List<UserOtpCode>();

        public virtual ICollection<UserSetting> UserSettings { get; set; } = new List<UserSetting>();

        public virtual ICollection<UserPlatform> UserPlatforms { get; set; } = new List<UserPlatform>();

        [Column(Order = 101)]
        public Guid CreatedUserId { get; set; }

        [Column(Order = 102)]
        public Guid? UpdatedUserId { get; set; }

        [Column(Order = 103)]
        public Guid? DeletedUserId { get; set; }

        [Column(Order = 104)]
        [MaxLength(100)]
        public string? CreatedFullName { get; set; }

        [Column(Order = 105)]
        [MaxLength(100)]
        public string? UpdatedFullName { get; set; }

        [Column(Order = 106)]
        [MaxLength(100)]
        public string? DeletedFullName { get; set; }

        [Column(Order = 107)]
        public DateTime CreatedDate { get; set; }

        [Column(Order = 108)]
        public DateTime? UpdatedDate { get; set; }

        [Column(Order = 109)]
        public DateTime? DeletedDate { get; set; }

        [Column(Order = 110)]
        [DefaultValue("false")]
        public bool IsDeleted { get; set; }
    }
}
