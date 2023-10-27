// Copyright (c) Atlantic. All rights reserved.

using Fsel.Shared.Enums;

namespace Fsel.Identity.Domain.Models.CommandModels.Auths
{
    public class LoginCommandModel
    {
        public string? Username { get; set; }

        public string? Password { get; set; }

        public EnumPlatformCode? PlatformCode { get; set; }
    }
}
