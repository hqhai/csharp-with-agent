// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Domain.Model.QueryModels
{
    using System;
    using Fsel.Shared.Enums;
    using Fsel.Core.Base.BaseModels;

    public class SearchNotificationModel : BaseQueryModel
    {
        public Guid? UserId { get; set; }
        public Guid? RoleId { get; set; }

        public EnumNotificationType Type { get; set; }
    }
}
