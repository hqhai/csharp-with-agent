
using Fsel.Core.Entities;
using Fsel.Shared.Enums;

namespace Fsel.Notification.Domain.Entities
{
    public class Notifications : Entity
    {
        public Guid UserId { get; set; } = Guid.Empty;

        public Guid RoleId { get; set; } = Guid.Empty;

        public EnumNotificationStatus Status { get; set; }

        public string? Title { get; set; }

        public string? Message { get; set; }

        public Guid ObjectId { get; set; }

        public Guid NotificationTypeId { get; set; }

        public NotificationType? NotificationType { get; set; }
    }
}
