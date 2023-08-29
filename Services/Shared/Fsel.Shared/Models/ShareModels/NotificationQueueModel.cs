// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using System;
    public class NotificationQueueModel
    {
        public Guid ObjectId { get; set; }

        public string? Message { get; set; }

        public Guid UserId { get; set; }
    }
}
