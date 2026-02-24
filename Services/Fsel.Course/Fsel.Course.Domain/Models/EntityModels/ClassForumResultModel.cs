// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;

    public class ClassForumResultModel : BaseModel
    {
        public double CorrectCount { get; set; }
        public double CorrectTotal { get; set; }
        public IList<SkillScores>? SkillScores { get; set; }

        public string? Content { get; set; }

        public Guid GradingTeacherId { get; set; }

        public Guid CourseId { get; set; }

        public Guid UnitId { get; set; }
        public Guid CurrentUnitId { get; set; }

        public EnumClassForumResultStatus? Status { get; set; }

        public Guid LessonResultId { get; set; }

        public Guid StudentId { get; set; }

        public Guid ClassForumId { get; set; }

        public int? FeedBackStars { get; set; }

        public int? CommentNumber { get; set; }

        public int? LikeNumber { get; set; }

        public string? FeedBackNote { get; set; }

        public bool? IsLiked { get; set; }

        public string? PostArea { get; set; }

        public bool IsTurnedOffNotification { get; set; }

        public string? WordContent { get; set; }
        public int? WordCount { get; set; }
        public int? TimeCount { get; set; }

        public string? GradingAlFeedback { get; set; }
        public string? AITranslationContent { get; set; }

        private string? _avatarPath;

        public string? AvatarPath
        {
            set { _avatarPath = value; }
            get { return _avatarPath.AddS3BaseUrl(); }
        }

        public EnumCourseLevel? CourseLevel { get; set; }
        public Guid? CheckCsoId { get; set; }
        public DateTime? CheckStartDate { get; set; }
        public DateTime? GradingStartDate { get; set; }
        public bool IsAIFeedBack { get; set; }
        public bool IsTeacherFeedBack { get; set; }
        public ClassForumModel? ClassForum { get; set; }
        public LessonResultModel? LessonResult { get; set; }
        public string? CourseCode { get; set; }
        public int LessonDisplayOrder { get; set; }
        public int UnitDisplayOrder { get; set; }
        public IList<EnumFeedBackPositive>? FeedBackPositives { get; set; }
        public IList<EnumFeedBackNegative>? FeedBackNegatives { get; set; }

        public bool IsViewed { get; set; }
        public int? TokenFirstTime { get; set; }
        public int? TokenLastTime { get; set; }

        public IList<string>? FilePaths
        { get { return ClassForumResultFiles?.Select(x => x.FilePath ?? string.Empty).ToList(); } }

        public IList<string>? RetryFilePaths
        { get { return ClassForumResultFiles?.Select(x => x.FilePath ?? string.Empty).ToList(); } }

        [JsonIgnore]
        public IList<ClassForumResultFileModel>? ClassForumResultFiles { get; set; }

        public IList<ClassForumScoreModel>? ClassForumScores { get; set; }
        public IList<ClassForumDetailResultModel>? ClassForumDetailResults { get; set; }
    }
}
