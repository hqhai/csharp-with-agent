// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Domain.Enums;

    public class UserOtpCodeModel : BaseModel
    {
        public string? OTPCode { get; set; }
        public DateTime ExpiredTime { get; set; }
        public EnumOtpCodeStatus Status { get; set; }
        public Guid? UserId { get; set; }
    }
}
