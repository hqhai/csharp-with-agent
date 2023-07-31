// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using System.Collections.Generic;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class ClassForumResultModel : BaseModel
    {
        public string? Content { get; set; }

        public Guid GradingTeacherId { get; set; }

        public EnumClassForumResultStatus Status { get; set; }

        public Guid LessonResultId { get; set; }

        public Guid StudentId { get; set; }

        public Guid ClassForumId { get; set; }

        public int? FeedBackStars { get; set; }

        public int? CommentNumber { get; set; }

        public int? LikeNumber { get; set; }

        public string? FeedBackNote { get; set; }

        public bool? IsFlagged { get; set; }

        public bool? IsLiked { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public ClassForumModel? ClassForum { get; set; }

        public LessonResultModel? LessonResult { get; set; }

        public IList<ClassForumResultFileModel>? ClassForumResultFiles { get; set; }

        public IList<ClassForumScoreModel>? ClassForumScores { get; set; }
    }
}
