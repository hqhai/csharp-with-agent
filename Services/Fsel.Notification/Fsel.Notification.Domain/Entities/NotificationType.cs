// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Domain.Entities
{
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class NotificationType : Entity
    {

        public string? Icon { get; set; }

        public EnumNotificationContent Content { get; set; }

        public EnumNotificationPushingType Type { get; set; }

        public int Priority { get; set; }

        public string? Template { get; set; }

    }
}
