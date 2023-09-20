// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Domain.Model.CommandModels.Notification
{

    public class UpdateStatusNotificationCommandModel
    {
        public bool MarkReadAll { get; set; }
        public IList<Guid> NotificationMessageIds { get; set; } = new List<Guid>();
    }
}
