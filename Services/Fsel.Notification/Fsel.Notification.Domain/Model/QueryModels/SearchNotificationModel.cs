// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Domain.Model.QueryModels
{
    using Fsel.Shared.Enums;
    using Fsel.Core.Base.BaseModels;

    public class SearchNotificationModel : BaseQueryModel
    {
        public EnumNotificationStatus? Status { get; set; }
    }
}
