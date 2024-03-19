// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.ErrorReports
{
    using Fsel.System.Domain.Enums;
    using global::System;
    using global::System.Collections.Generic;

    public class CreateErrorReportCommandModel
    {
        public EnumTypeOfError Type { get; set; }

        public Guid? CourseId { get; set; }

        public Guid? UnitId { get; set; }

        public Guid? LessonId { get; set; }

        public string? Url { get; set; }

        public EnumFeaturePlatform? FeaturePlatform { get; set; }

        public EnumFeatureLearn? FeatureLearn { get; set; }

        public IList<string>? ImagePaths { get; set; }

        public string? Content { get; set; }
    }
}
