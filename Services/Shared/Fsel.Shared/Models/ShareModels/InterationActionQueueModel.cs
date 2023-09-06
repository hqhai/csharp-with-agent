// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using Fsel.Shared.Enums;

    public class InterationActionQueueModel
    {
        public Guid ObjectId { get; set; }

        public EnumInteractionActionType Type { get; set; }

        public Guid UserId { get; set; }

        public string? AvatarPath { get; set; }

        public string? FullName { get; set; }
    }

}
