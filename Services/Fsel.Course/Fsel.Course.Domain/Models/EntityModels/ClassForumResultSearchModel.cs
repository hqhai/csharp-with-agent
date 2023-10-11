// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

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

        public string? PostArea { get; set; }
        public Guid? TeacherId { get; set; }
        public Guid? CourseId { get; set; }
        public EnumCourseSkill CourseSkill { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public EnumCourseType CourseType { get; set; }
        public ClassForumModel? ClassForum { get; set; }

        public LessonResultModel? LessonResult { get; set; }
        public string? CourseCode { get; set; }
        public string? ClassCode { get; set; }
        public int LessonDisplayOrder { get; set; }
        public int UnitDisplayOrder { get; set; }

        public Guid? CheckCsoId { get; set; }

        public DateTime? CheckStartDate { get; set; }

        public DateTime? GradingStartDate { get; set; }

        public Guid? CsoId { get; set; }
        public IList<string>? FilePaths { get; set; }

        public IList<ClassForumScoreModel>? ClassForumScores { get; set; }
    }
}
