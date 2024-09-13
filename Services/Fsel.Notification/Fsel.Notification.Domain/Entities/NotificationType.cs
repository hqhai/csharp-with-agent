// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class NotificationType : Entity
    {
        public string? Icon { get; set; }

        public EnumNotificationContent Content { get; set; }

        public EnumNotificationType Type { get; set; }

        public int Priority { get; set; }

        public string? TemplateMessage { get; set; }

        public string? TemplateLink { get; set; }
        public ICollection<NotificationTypeTranslation> Translations { get; set; } = new List<NotificationTypeTranslation>();

    }

    public class NotificationTypeTranslation : Entity, ITranslationObject
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? TemplateMessage { get; set; }

        public Guid NotificationTypeId { get; set; }

        public NotificationType? NotificationType { get; set; }

        public string? Language { get; set; }
    }
}
