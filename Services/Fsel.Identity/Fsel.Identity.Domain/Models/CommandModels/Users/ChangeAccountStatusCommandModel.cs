// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Users
{
    using Fsel.Shared.Enums;

    public class ChangeAccountStatusCommandModel
    {
        public IList<Guid>? UserIds { get; set; }
        public EnumPlatformCode PlatformCode { get; set; }
        public EnumUserPlatformStatus Status { get; set; }
    }
}
