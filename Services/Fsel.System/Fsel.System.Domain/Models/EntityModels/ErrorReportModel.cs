// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Enums;
    using global::System;
    using global::System.Collections.Generic;

    public class ErrorReportModel : BaseModel
    {
        public EnumTypeOfError Type { get; set; }

        public Guid? CourseId { get; set; }

        public Guid? UnitId { get; set; }

        public Guid? LessonId { get; set; }

        public EnumPriority Priority { get; set; }

        public EnumErrorReportStatus Status { get; set; }

        public string? Url { get; set; }

        public string? FeedBack { get; set; }

        public EnumFeaturePlatForm? FeaturePlatform { get; set; }

        public EnumFeatureLearn? FeatureLearn { get; set; }

        public IList<string>? ImagePaths { get; set; }

        public string? Content { get; set; }

        public EnumCourseLevel? CourseLevel { get; set; }

        public EnumCourseType? CourseType { get; set; }

        public string? CourseName { get; set; }

        public string? UnitName { get; set; }

        public string? LessonName { get; set; }
    }
}
