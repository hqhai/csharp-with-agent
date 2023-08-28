// Copyright (c) Atlantic. All rights reserved.

namespace FiveSIS.Notification.Domain.Model.CommandModels.Notification
{
    using System;

    public class CreateNotificationCommandModel
    {
        public string? Title { get; set; }
        public string? Message { get; set; }

        public string? RoleIds { get; set; }

        public Guid UserId { get; set; } = Guid.Empty;

        public Guid NotificationTypeId { get; set; } = Guid.Empty;
    }
}
