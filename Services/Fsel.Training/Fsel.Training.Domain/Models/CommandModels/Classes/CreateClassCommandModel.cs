// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.CommandModels.Classes
{
    using Fsel.Shared.Enums;

    public class CreateClassCommandModel
    {
        public EnumCourseLevel CourseLevel { get; set; }
        public string? CourseName { get; set; }
        public Guid PackageId { get; set; }
        public Guid CourseId { get; set; }
        public Guid? LiveTimeFrameId { get; set; }
        public IList<DayOfWeek>? LiveDays { get; set; }
    }
}
