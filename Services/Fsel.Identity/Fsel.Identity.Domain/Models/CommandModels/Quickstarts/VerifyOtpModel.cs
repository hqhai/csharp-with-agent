// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Quickstarts
{
    public class VerifyOtpModel
    {
        //[Required(ErrorMessage = "i18n_OTP_cannot_be_empty")]
        public string? Otp { get; set; }

        public string? ReturnUrl { get; set; }

        public string? Type { get; set; }
    }
}
