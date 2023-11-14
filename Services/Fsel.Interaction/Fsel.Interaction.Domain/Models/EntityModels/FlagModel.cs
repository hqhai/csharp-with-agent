// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class FlagModel : BaseModel
    {
        public EnumFlagIssue FlagIssue { get; set; }

        public string? FeedBack { get; set; }

        public EnumFlagStatus Status { get; set; }

        public EnumInteractionType Type { get; set; }
        public Guid? ObjectId { get; set; }

        public string? Content { get; set; }
        public string? CreatedUserName { get; set; }
        public Guid? StudentId { get; set; }
        public Guid? UserId { get; set; }
        public string? AvatarPath { get; set; }
        public Guid? ClassForumResultId { get; set; }
    }
}
