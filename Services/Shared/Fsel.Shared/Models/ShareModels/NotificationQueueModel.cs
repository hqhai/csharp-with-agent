// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using System;
    using Fsel.Shared.Enums;

    public class NotificationQueueModel
    {
        public EnumNotificationType Type { get; set; }

        public EnumNotificationContent Content { get; set; }
        public Guid ObjectId { get; set; }

        public string? Message { get; set; }

        public string? Link { get; set; }

        public Guid? UserId { get; set; }

        public string? Template { get; set; }

        public string? Icon { get; set; }

        public IList<Guid>? UserIds { get; set; }

        public IList<EnumRole>? Roles { get; set; }

        public Guid NotificationTypeId { get; set; }

        public IList<object>? ParamsMessage { get; set; }
        public IList<object>? ParamsLink { get; set; }

        public Guid? SenderId { get; set; }
        public string? AvatarPath { get; set; }

        public EnumNotificationStatus Status { get; set; }
    }
}
