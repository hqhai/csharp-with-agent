// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Domain.Model.EntityModels
{
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class NotificationMessageModel : BaseModel
    {
        public Guid? UserId { get; set; }
        public Guid? SenderId { get; set; }

        public Guid? RoleId { get; set; }

        public EnumNotificationStatus Status { get; set; }

        public string? Message { get; set; }

        public string? Link { get; set; }

        public Guid ObjectId { get; set; }

        public Guid NotificationTypeId { get; set; }

        private string? _avatarPath;
        public string? AvatarPath
        {
            set { _avatarPath = value; }
            get { return _avatarPath.AddS3BaseUrl(); }
        }

        public EnumNotificationContent Content { get; set; }

        public EnumNotificationType Type { get; set; }

        public IList<Guid>? UserIds { get; set; }

        public string? Icon { get; set; }

        public NotificationsTypeModel? NotificationType { get; set; }
    }

}
