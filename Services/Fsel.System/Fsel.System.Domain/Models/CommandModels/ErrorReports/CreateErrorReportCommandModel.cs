// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.ErrorReports
{
    using Fsel.System.Domain.Enums;
    using global::System;
    using global::System.Collections.Generic;

    public class CreateErrorReportCommandModel
    {
        public EnumTypeOfError TypeOfError { get; set; }

        public Guid? CourseId { get; set; }

        public Guid? UnitId { get; set; }

        public Guid? LessonId { get; set; }

        public string? Url { get; set; }

        public EnumPlatFormDetail? PlatFormDetail { get; set; }

        public EnumLessonDetail? LessonDetail { get; set; }

        public IList<string>? ImageLinks { get; set; }

        public string? StudentFeedBack { get; set; }
    }
}
