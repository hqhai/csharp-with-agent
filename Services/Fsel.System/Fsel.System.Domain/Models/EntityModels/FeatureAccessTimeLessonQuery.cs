// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    public class FeatureAccessTimeLessonQuery
    {
        public Guid CourseId { get; set; }
        public long AccessTime { get; set; }
        public int TotalVisit { get; set; }
        public DateTime? LastVisited { get; set; }
    }
}
