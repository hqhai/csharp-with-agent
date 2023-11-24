// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Domain.Model.CommandModels.Notification
{
    using System;
    using Fsel.Shared.Enums;

    public class CreateNotificationCommandModel
    {
        public string? Message { get; set; }

        public string? Link { get; set; }

        public IList<EnumRole>? Roles { get; set; }

        public IList<Guid>? UserIds { get; set; }

        public Guid NotificationTypeId { get; set; }

        public Guid ObjectId { get; set; }

        public Guid? SenderId { get; set; }
    }
}
