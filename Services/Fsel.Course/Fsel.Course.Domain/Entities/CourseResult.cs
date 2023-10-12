// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    public class CourseResult : BaseResultScore
    {
        public Course? Course { get; set; }
        public Guid CourseId { get; set; }
    }
}
