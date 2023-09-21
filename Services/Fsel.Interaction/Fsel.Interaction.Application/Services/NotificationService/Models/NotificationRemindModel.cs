// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.NotificationService.Models
{
    using Fsel.Shared.Enums;

    public class NotificationRemindModel
    {
        public Guid ObjectId { get; set; }

        public Guid UserId { get; set; }

        public EnumNotificationRemindStatus Status { get; set; }
    }
}
