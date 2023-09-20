// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService.Models
{
    using Fsel.Shared.Enums;

    public class FeatureAccessTimeCourseModel
    {
        public EnumFeature EnumFeature { get; set; }
        public int Visit { get; set; }
        public long AccessTime { get; set; }
        public DateTime LastVisited { get; set; }
        public Guid ObjectId { get; set; }
        public Guid CourseId { get; set; }
        public Guid? UnitId { get; set; }
        public Guid? LessonId { get; set; }
    }
}
