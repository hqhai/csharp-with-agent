// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using Fsel.Shared.Enums;

    public class NotificationTypeTextModel
    {
        public string? Title { get; set; }
        public Guid ObjectId { get; set; }
        public string? Message { get; set; }
        public Guid? UserId { get; set; }
        public IList<Guid>? UserIds { get; set; }
        public List<EnumRole>? Roles { get; set; }
    }
}
