// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.TrainingService.CommandModels
{
    using System;
    using Fsel.Shared.Enums;

    public class RegisterClassCommandModel
    {
        public Guid PackageId { get; set; }
        public Guid CourseId { get; set; }
        public string? CodeCourse { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public Guid? LiveTimeFrameId { get; set; }
        public IList<DayOfWeek>? LiveDays { get; set; }
        public Guid UserId { get; set; }
    }
}
