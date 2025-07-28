// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.OpenId
{
    public class VerifyOtpModel
    {
        //[Required(ErrorMessage = "i18n_OTP_cannot_be_empty")]
        public string? Otp { get; set; }

        public string? Identity { get; set; }

        public string? ReturnUrl { get; set; }

        public string? Type { get; set; }

        public DateTime? ExpiredTime { get; set; }

        public long? RemainSecond
        {
            get
            {
                var dateNow = DateTime.UtcNow;
                if (ExpiredTime.HasValue && ExpiredTime.Value > dateNow)
                {
                    return (long)(ExpiredTime.Value - dateNow).TotalSeconds;
                }

                return default;
            }
        }
    }
}
