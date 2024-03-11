// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.QueryModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Enums;

    public class SearchErrorReportQueryModel : BaseQueryModel
    {
        public EnumTypeOfError? Type { get; set; }

        public Guid? CourseId { get; set; }

        public Guid? UnitId { get; set; }

        public Guid? LessonId { get; set; }

        public EnumPriority? Priority { get; set; }

        public EnumErrorReportStatus? Status { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public EnumCourseType? CourseType { get; set; }
        public EnumFeaturePlatform? FeaturePlatform { get; set; }
        public EnumFeatureLearn? FeatureLearn { get; set; }
    }
}
