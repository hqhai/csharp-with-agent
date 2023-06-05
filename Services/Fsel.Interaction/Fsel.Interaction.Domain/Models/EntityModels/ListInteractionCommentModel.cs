// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class ListCommentModel : BaseModel
    {
        public string? Content { get; set; }
        public EnumCommentStatus Status { get; set; }
        public int LikeNumber { get; set; }
        public Guid ObjectId { get; set; }
        public Guid UserId { get; set; }
        public bool IsLiked { get; set; }
        public string? AvatarPath { get; set; }

        public string? FullName { get; set; }

        public EnumInteractionActionType? InteractionActionType { get; set; }
        public IList<CommentModel>? Comments { get; set; }
    }
}
