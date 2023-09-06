// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Domain.Model.CommandModels.Notification
{
    using System;
    using Fsel.Shared.Enums;

    public class CreateNotificationCommandModel
    {
        public string? Title { get; set; }
        public string? Message { get; set; }
        public IList<EnumRole>? Roles { get; set; }
        public Guid UserId { get; set; } = Guid.Empty;
        public Guid NotificationTypeId { get; set; }
        public Guid ObjectId { get; set; }
    }
}
