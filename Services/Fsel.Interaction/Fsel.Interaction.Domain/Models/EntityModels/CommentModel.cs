// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    using System;
    using Fsel.Common.Helpers;
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

        private string? _avatarPath;
        public string? AvatarPath
        {
            set { _avatarPath = value; }
            get { return _avatarPath.AddS3BaseUrl(); }
        }
        public string? FullName { get; set; }
        public EnumInteractionType Type { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public EnumInteractionActionType? InteractionActionType { get; set; }
        public IList<CommentModel>? Comments { get; set; }
        public EnumCommentStatus Status { get; set; }
    }
}
