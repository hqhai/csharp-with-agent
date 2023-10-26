// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.CommandModels.Flags
{
    using System;
    using Fsel.Interaction.Domain.Enums;
    using Fsel.Shared.Enums;

    public class RateFlagCommandModel
    {
        public EnumFlagIssue FlagIssue { get; set; }

        public string? FeedBack { get; set; }

        public EnumInteractionType Type { get; set; }
        public Guid ObjectId { get; set; }
    }
}
