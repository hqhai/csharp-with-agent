// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using System;
    using Fsel.Shared.Enums;

    public class NotificationAllQueueModel
    {
        public string? Title { get; set; }
        public Guid ObjectId { get; set; }

        public string? Message { get; set; }

        public Guid? UserId { get; set; }

        public string? Template { get; set; }

        public string? Icon { get; set; }

        public IList<Guid>? UserIds { get; set; }

        public List<EnumRole>? Roles { get; set; }

        public Guid? NotificationTypeId { get; set; }
    }
}
