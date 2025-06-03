// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Shared.Enums;

    public class UserOtpCode : Entity
    {
        [MaxLength(20, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? OTPCode { get; set; }

        public DateTime ExpiredTime { get; set; }

        public int RetryCount { get; set; }

        public virtual User? User { get; set; }

        public EnumOtpCodeStatus Status { get; set; }

        public EnumUserOtpCodeType Type { get; set; }

        public Guid? UserId { get; set; }
    }
}
