// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using Fsel.Core.Entities;

    public class UnitStudent : Entity
    {
        public long? Result { get; set; }

        public Unit? Unit { get; set; }
        public Guid UnitId { get; set; }

        public Course? Course { get; set; }
        public Guid CourseId { get; set; }

        public Guid StudentId { get; set; }
    }
}
