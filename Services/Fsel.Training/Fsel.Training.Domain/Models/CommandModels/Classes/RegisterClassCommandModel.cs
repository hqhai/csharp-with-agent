// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.CommandModels.Classes
{
    public class RegisterClassCommandModel
    {
        public Guid PackageId { get; set; }
        public Guid CourseId { get; set; }
        public Guid? LiveTimeFrameId { get; set; }
        public IList<DayOfWeek>? LiveDays { get; set; }
        public Guid? UserId { get; set; }
    }
}
