// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService.Models
{
    public class FeatureAccessTimeCourseModel
    {
        public Guid CourseId { get; set; }
        public long AccessTime { get; set; }
        public int TotalVisit { get; set; }
        public DateTime LastVisited { get; set; }
    }
}
