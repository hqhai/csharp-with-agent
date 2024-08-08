// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Domain.Model.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class PagingItemsNotificationModel : PagingItemsModel<NotificationMessageModel>
    {
        public int? TotalRead { get; set; }
        public int? TotalSent { get; set; }
    }
}
