// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.OpenId
{
    using Fsel.Core.Base.Interfaces;

    public class VerifyOtpModel : IRequestBodyTenantAware
    {
        //[Required(ErrorMessage = "i18n_OTP_cannot_be_empty")]
        public string? Otp { get; set; }

        public string? Identity { get; set; }

        public string? ReturnUrl { get; set; }

        public string? Type { get; set; }

        public DateTime? ExpiredTime { get; set; }

        public string? OtpProvider { get; set; }

        public long? RemainSecond
        {
            get
            {
                if (ExpiredTime.HasValue)
                {
                    return (long)(ExpiredTime.Value - DateTime.UtcNow).TotalSeconds;
                }
                return default;
            }
        }

        public string? UserName { get; set; }

        public Guid? UserId { get; set; }
    }
}
