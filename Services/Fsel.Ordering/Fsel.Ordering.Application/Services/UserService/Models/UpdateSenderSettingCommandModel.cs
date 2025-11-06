// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.UserService.Models
{
    using Fsel.Shared.Enums;

    public class UpdateSenderSettingCommandModel
    {
        public Guid UserId { get; set; }
        public EnumSenderTemplate Template { get; set; }
    }
}
