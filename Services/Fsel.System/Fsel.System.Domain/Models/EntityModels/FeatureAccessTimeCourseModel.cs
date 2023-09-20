// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    public class FeatureAccessTimeCourseModel
    {
        public int Visit { get; set; }
        public long AccessTime { get; set; }
        public DateTime LastVisited { get; set; }
        public Guid ObjectId { get; set; }
        public Guid CourseId { get; set; }
    }
}
