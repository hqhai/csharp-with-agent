// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    public class ClassForumCommentQueueModel
    {
        public Guid ObjectId { get; set; }

        public Guid UserId { get; set; }

        public string? AvatarPath { get; set; }

        public string? FullName { get; set; }
    }

}
