// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    public class NotificationQueueModel
    {
        public string? AvatarPath { get; set; }

        public IList<Guid>? UserIds { get; set; }

        public string? Message { get; set; }

        public string? Link { get; set; }
    }
}
