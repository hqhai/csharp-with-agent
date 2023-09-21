// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.NotificationServices.Models
{
    using Fsel.Shared.Enums;

    public class GetListNotificationRemindCommand
    {
        public IList<Guid>? ObjectIds { get; set; }
        public EnumNotificationRemindStatus Status { get; set; }
    }
}
