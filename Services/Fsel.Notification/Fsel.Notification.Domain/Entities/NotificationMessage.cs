
using Fsel.Core.Base.Interfaces;
using Fsel.Core.Entities;
using Fsel.Shared.Enums;

namespace Fsel.Notification.Domain.Entities
{
    public class NotificationMessage : Entity
    {
        public Guid? UserId { get; set; }
        public Guid? SenderId { get; set; }

        public EnumNotificationStatus Status { get; set; }

        public string? Message { get; set; }

        public string? Link { get; set; }

        public Guid ObjectId { get; set; }

        public Guid NotificationTypeId { get; set; }

        public NotificationType? NotificationType { get; set; }

        public ICollection<NotificationMessageTranslation> Translations { get; set; } = new List<NotificationMessageTranslation>();

    }

    public class NotificationMessageTranslation : Entity, ITranslationObject
    {
        public string? Language { get; set; }

        public string? Message { get; set; }

        public NotificationMessage? NotificationMessage { get; set; }

        public Guid NotificationMessageId { get; set; }
    }
}
