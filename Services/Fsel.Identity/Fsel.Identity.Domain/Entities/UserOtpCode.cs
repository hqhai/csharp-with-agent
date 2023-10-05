// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using Fsel.Core.Entities;
    using Fsel.Identity.Domain.Enums;

    public class UserOtpCode : Entity
    {
        public string? OTPCode { get; set; }
        public DateTime ExpiredTime { get; set; }
        public virtual User? User { get; set; }
        public EnumOtpCodeStatus Status { get; set; }
        public string? UserId { get; set; }
    }
}
