// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.NotificationService.Models
{
    using Fsel.Shared.Enums;

    public class GetListNotificationRemindQueryModel
    {
        public IList<Guid>? ObjectIds { get; set; }
        public EnumNotificationRemindStatus Status { get; set; }
    }
}
