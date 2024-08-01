// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IEntities;
    using Fsel.Shared.Enums;

    public class CourseManagerModel : IModuleLifeCycle
    {
        public Guid CourseResultId { get; set; }
        public EnumResultStatus Status { get; set; }
        public double Percent { get; set; }
        public EnumWorkingStatus WorkingStatus { get; set; }
        public DateTime? ProcessDate { get; set; }
        public DateTime? CompletionDate { get; set; }

        public Guid CourseId { get; set; }
        public string? CodeCourse { get; set; }
        public EnumCourseType? CourseType { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }

        public long TimeSpent { get; set; }

        public string? Type { get; set; }
        public Guid? ObjectId { get; set; }
        public int DisplayOrder { get; set; }

        public Guid? ObjectLessonId { get; set; }
        public string? LessonType { get; set; }
        public int? LessonDisplayOrder { get; set; }
        public EnumCourseSkill? CourseSkill { get; set; }

        public double ProgressPercent { get; set; }
        public bool? IsCheckPercentColor { get; set; }
        public bool IsHiddenCourseLevel { get; set; }
        public bool IsChangeLevel { get; set; } = true;
        public bool IsResetCourse { get; set; } = true;
    }
}
