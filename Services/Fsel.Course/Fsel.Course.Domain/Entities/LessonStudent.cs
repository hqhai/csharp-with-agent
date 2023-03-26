// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Core.Entities;

    public class LessonStudent : Entity
    {
        public long? Result { get; set; }

        public long TimeWatchVideo { get; set; }

        public Unit? Unit { get; set; }
        public Guid LessonId { get; set; }
        public Lesson? Lesson { get; set; }
        public Guid UnitId { get; set; }
        public Course? Course { get; set; }
        public Guid CourseId { get; set; }

        public Guid StudentId { get; set; }

        [NotMapped]
        public TimeSpan TimeWatchVideoSpan
        {
            get { return TimeSpan.FromTicks(TimeWatchVideo); }
        }
    }
}
