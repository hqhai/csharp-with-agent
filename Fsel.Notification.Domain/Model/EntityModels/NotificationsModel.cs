// Copyright (c) Atlantic. All rights reserved.

namespace FiveSIS.Notification.Domain.Model.EntityModels
{
    using FiveSIS.Shared.Enums;
    using Fsel.Core.Base.BaseModels;

    public class NotificationsModel : BaseModel
    {

        public Guid? UserId { get; set; }

        public Guid? RoleId { get; set; }

        public EnumNotificationStatus Status { get; set; }

        public string? Title { get; set; }

        public string? Message { get; set; }

        public Guid? ObjectId { get; set; }

        public Guid NotificationTypeId { get; set; }

        public string? Template { get; set; }
    }
}
