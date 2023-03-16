// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Core.Entities;

    public class CourseTeacher : Entity
    {
        public Course? Course { get; set; }
        public Guid TeacherId { get; set; }

        public Guid CourseId { get; set; }
    }
}
