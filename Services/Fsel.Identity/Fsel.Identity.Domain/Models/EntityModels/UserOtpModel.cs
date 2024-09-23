// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Domain.Enums;

    public class UserOtpModel : BaseModel
    {
        public string? Otp { get; set; }

        public DateTime ExpiredTime { get; set; }

        public EnumUserOtpStatus Status { get; set; }

        public Guid? VerifyId { get; set; }

        public UserModel? User { get; set; }

        public Guid? UserId { get; set; }
    }
}
