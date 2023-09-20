// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Domain.Model.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class NotificationRemindModel : BaseModel
    {
        public Guid ObjectId { get; set; }

        public Guid UserId { get; set; }

        public EnumNotificationRemindStatus Status { get; set; }

    }
}
