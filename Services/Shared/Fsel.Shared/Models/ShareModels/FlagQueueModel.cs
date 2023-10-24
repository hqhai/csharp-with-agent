// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class FlagQueueModel : BaseModel
    {
        public EnumFlagIssue FlagIssue { get; set; }

        public string? FeedBack { get; set; }

        public EnumFlagStatus Status { get; set; }

        public EnumInteractionType Type { get; set; }
        public IList<Guid>? ObjectIds { get; set; }
    }
}
