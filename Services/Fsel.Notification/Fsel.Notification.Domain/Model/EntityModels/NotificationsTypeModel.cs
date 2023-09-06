// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Domain.Model.EntityModels
{
    using Fsel.Shared.Enums;
    using Fsel.Core.Base.BaseModels;

    public class NotificationsTypeModel : BaseModel
    {
        public string? Icon { get; set; }

        public EnumNotificationContent Content { get; set; }

        public EnumNotificationPushingType Type { get; set; }

        public bool Active { get; set; }

        public int Priority { get; set; }

        public string? Template { get; set; }
       
    }
}
