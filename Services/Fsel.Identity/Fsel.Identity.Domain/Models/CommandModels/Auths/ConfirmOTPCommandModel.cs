// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Auths
{
    public class ConfirmOTPCommandModel
    {
        public string? Code { get; set; }
        public string? Email { get; set; }
    }
}
