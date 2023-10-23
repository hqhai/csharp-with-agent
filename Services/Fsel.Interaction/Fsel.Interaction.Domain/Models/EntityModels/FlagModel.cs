// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Interaction.Domain.Enums;
    using Fsel.Shared.Enums;

    public class FlagModel : BaseModel
    {
        public EnumFlagIssue FlagIssue { get; set; }

        public string? FeedBack { get; set; }

        public EnumFlagStatus Status { get; set; }

        public EnumInteractionType Type { get; set; }
        public Guid? ObjectId { get; set; }
    }
}
