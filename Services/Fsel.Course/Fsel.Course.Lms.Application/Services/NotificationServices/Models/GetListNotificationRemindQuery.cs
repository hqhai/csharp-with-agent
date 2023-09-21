// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.NotificationServices.Models
{
    using Fsel.Shared.Enums;

    public class GetListNotificationRemindQuery
    {
        public IList<Guid>? ObjectIds { get; set; }
        public EnumNotificationRemindStatus Status { get; set; }
    }
}
