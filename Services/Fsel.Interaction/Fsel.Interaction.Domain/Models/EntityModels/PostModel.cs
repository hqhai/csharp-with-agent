// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Shared.Enums;

    public class PostModel : BaseModel
    {
        public string? Title { get; set; }

        public string? Content { get; set; }

        public EnumPostStatus Status { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public Guid UserId { get; set; }

        private string? _avatarPath;
        public string? AvatarPath
        {
            set { _avatarPath = value; }
            get { return _avatarPath.AddS3BaseUrl(); }
        }

        public string? FullName { get; set; }

        public IList<string>? FilePaths { get; set; }

        public IList<TopicTagModel>? TopicTags { get; set; }

        public IList<CommentModel>? Comments { get; set; }

        public IList<InteractionActionModel>? ActionLikes { get; set; }

    }
}
