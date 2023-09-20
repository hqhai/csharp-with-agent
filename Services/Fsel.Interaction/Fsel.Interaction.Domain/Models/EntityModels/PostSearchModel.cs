// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;   
    using Fsel.Shared.Enums;

    public class PostSearchModel : BaseModel
    {
        public string? Title { get; set; }

        public string? Content { get; set; }

        public EnumPostStatus Status { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public Guid UserId { get; set; }

        public string? AvatarPath { get; set; }

        public string? FullName { get; set; }

        public IList<string>? FilePaths { get; set; }

        public IList<TopicTagModel>? PostTags { get; set; }

        public int? LikeCount { get; set; }

        public int? CommentCount { get; set; }

        public bool IsLiked { get; set; }
    }
}
