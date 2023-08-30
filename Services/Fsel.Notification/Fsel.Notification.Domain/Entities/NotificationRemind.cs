// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Domain.Entities
{
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class NotificationRemind : Entity
    {
        public Guid ObjectId { get; set; }

        public Guid UserId { get; set; }

        public EnumNotificationRemindStatus Status { get; set; }
    }
}
