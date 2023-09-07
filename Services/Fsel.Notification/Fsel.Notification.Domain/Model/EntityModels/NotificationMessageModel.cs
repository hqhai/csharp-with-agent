// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Domain.Model.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class NotificationMessageModel : BaseModel
    {

        public Guid? UserId { get; set; }

        public Guid? RoleId { get; set; }

        public EnumNotificationStatus Status { get; set; }


        public string? Message { get; set; }

        public Guid ObjectId { get; set; }

        public Guid NotificationTypeId { get; set; }

        public string? Template { get; set; }

        public string? Icon { get; set; }

        public IList<string>? UserIds { get; set; }
    }

}
