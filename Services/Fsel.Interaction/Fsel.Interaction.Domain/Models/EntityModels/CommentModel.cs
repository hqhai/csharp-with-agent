// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class CommentModel : BaseModel
    {
        public string? Content { get; set; }
        public bool? IsFlagged { get; set; }
        public int LikeNumber { get; set; }
        public int CommentNumber { get; set; }
        public Guid ObjectId { get; set; }
        public bool IsLiked { get; set; }
        public Guid UserId { get; set; }
        public string? AvatarPath { get; set; }
        public string? FullName { get; set; }
        public EnumCommentType Type { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public EnumInteractionActionType? InteractionActionType { get; set; }
        public IList<CommentModel>? Comments { get; set; }
    }
}
