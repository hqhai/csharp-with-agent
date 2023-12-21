// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using System;
    using System.Collections.Generic;
    using Fsel.Shared.Enums;

    public class NotificationSendingQueueModel
    {
        public IList<Guid>? UserIds { get; set; }

        public IList<EnumRole>? Roles { get; set; }

        public Guid SenderId { get; set; }

        public IList<object>? ParamsMessage { get; set; }

        public IList<object>? ParamsLink { get; set; }

        public Guid ObjectId { get; set; }

        public EnumNotificationType Type { get; set; }

        public EnumNotificationContent Content { get; set; }

        public EnumPlatformCode PlatformCode { get; set; }
    }
}
