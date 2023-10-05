// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.CommandModels.Classes
{
    using Fsel.Shared.Enums;

    public class RegisterClassCommandModel
    {
        public Guid PackageId { get; set; }
        public Guid CourseId { get; set; }
        public string? Code { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public Guid? LiveTimeFrameId { get; set; }
        public IList<DayOfWeek>? LiveDays { get; set; }
        #region
        public Guid? UserId { get; set; }
        #endregion
    }
}
