// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.InteractionService.Models
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;

    public class FlagModel : BaseModel
    {
        public EnumFlagIssue FlagIssue { get; set; }

        public string? FeedBack { get; set; }

        public EnumFlagStatus Status { get; set; }

        public EnumInteractionType Type { get; set; }
        public Guid? ObjectId { get; set; }
    }
}
