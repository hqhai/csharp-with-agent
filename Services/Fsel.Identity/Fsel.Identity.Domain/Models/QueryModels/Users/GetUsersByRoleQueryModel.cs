// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.QueryModels.Users
{
    using Fsel.Shared.Enums;

    public class GetUsersByRoleQueryModel
    {
        public EnumRole Role { get; set; }
    }
}
