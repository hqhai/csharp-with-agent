// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;

    public class ClassForumResultSearchModel : BaseModel
    {
        public string? Content { get; set; }

        public Guid GradingTeacherId { get; set; }

        public EnumClassForumResultStatus Status { get; set; }

        public Guid LessonResultId { get; set; }

        public Guid StudentId { get; set; }

        public Guid ClassForumId { get; set; }
        public string? LessonName { get; set; }

        public string? UnitName { get; set; }

        public Guid? TeacherId { get; set; }

        public ClassForumModel? ClassForum { get; set; }

        public LessonResultModel? LessonResult { get; set; }

        public IList<ClassForumResultFileModel>? ClassForumResultFiles { get; set; }

        public IList<ClassForumScoreModel>? ClassForumScores { get; set; }
    }
}
