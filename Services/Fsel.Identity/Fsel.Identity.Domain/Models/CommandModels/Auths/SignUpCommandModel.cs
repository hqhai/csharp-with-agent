// Copyright (c) Atlantic. All rights reserved.

using Fsel.Shared.Enums;

namespace Fsel.Identity.Domain.Models.CommandModels.Auths
{
    public class SignUpCommandModel
    {
        public string? FullName { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Email { get; set; }

        public string? Password { get; set; }

        public EnumRoleRegister Role { get; set; }

        public string? ReferralCode { get; set; }

        public EnumPlatformCode? PlatformCode { get; set; }
    }
}
