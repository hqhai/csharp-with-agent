// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Domain.Model.CommandModels.Notification
{
    using System;
    using Fsel.Shared.Enums;

    public class UpdateNotificationCommandModel
    {
        public string? Message { get; set; }

        public string? Link { get; set; }

        public Guid UserId { get; set; }

        public Guid NotificationTypeId { get; set; }

        public Guid ObjectId { get; set; }

        public Guid? SenderId { get; set; }
        public EnumNotificationStatus? Status { get; set; }
    }
}
