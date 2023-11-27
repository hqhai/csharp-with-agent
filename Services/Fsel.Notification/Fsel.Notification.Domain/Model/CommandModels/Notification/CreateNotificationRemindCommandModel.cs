// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Domain.Model.CommandModels.Notification
{
    using System;
    using Fsel.Shared.Enums;

    public class CreateNotificationRemindCommandModel
    {
        public IList<Guid>? UserIds { get; set; }
        public Guid ObjectId { get; set; }
        public EnumNotificationRemindStatus Status { get; set; }
    }
}
