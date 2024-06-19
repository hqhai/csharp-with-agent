// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Auths
{
    using Fsel.Shared.Enums;

    public class CreateGuestAccountCommandModel
    {
        public EnumPlatformCode PlatformCode { get; set; }
    }
}
