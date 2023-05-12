// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Auths
{
    public class ResetPasswordCommandModel
    {
        public string? Otp { get; set; }
        public string? NewPassword { get; set; }
    }
}
