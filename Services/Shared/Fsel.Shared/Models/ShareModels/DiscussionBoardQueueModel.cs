// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class DiscussionBoardQueueModel
    {
        public Guid ObjectId { get; set; }

        public long? LikeNumber { get; set; }

        public long? CommentNumber { get; set; }

        public CommentQueueModel? ChangeComment { get; set; }
    }


    public class CommentQueueModel : BaseModel
    {
        public string? Content { get; set; }
        public int LikeNumber { get; set; }
        public int CommentNumber { get; set; }
        public Guid ObjectId { get; set; }
        public Guid UserId { get; set; }
        public string? AvatarPath { get; set; }
        public string? FullName { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
    }
}
