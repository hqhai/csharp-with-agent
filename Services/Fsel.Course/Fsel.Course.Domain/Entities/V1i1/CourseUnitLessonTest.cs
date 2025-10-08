// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.V1i1
{
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Entities.TestConfigs;

    public class CourseUnitLessonTest : Entity
    {
        public Course? Course { get; set; }
        public Guid CourseId { get; set; }
        public Unit? Unit { get; set; }
        public Guid? UnitId { get; set; }
        public Lesson? Lesson { get; set; }
        public Guid? LessonId { get; set; }
        public MockTest? MockTest { get; set; }
        public Guid? MockTestId { get; set; }

        public ICollection<TestGroupResult> TestGroupResults { get; set; } = new List<TestGroupResult>();
    }
}
