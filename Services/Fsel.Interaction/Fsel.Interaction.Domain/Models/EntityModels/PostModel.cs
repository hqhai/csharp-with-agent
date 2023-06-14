// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    using System;
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

        public IList<string>? FilePaths { get; set; }

        public IList<TopicTag>? TopicTags { get; set; }

        public IList<CommentModel>? Comments { get; set; }
    }
}
