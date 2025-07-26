// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using Fsel.Core.Entities;

    public class UserSenderSetting : Entity
    {
        public Guid SenderConfigId { get; set; }

        public bool IsActive { get; set; }

        public Guid UserSettingId { get; set; }

        public UserSetting? UserSetting { get; set; }
    }
}
