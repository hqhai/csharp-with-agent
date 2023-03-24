// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;

    public class CourseClassModel
    {
        public Guid CourseId { get; set; }

        public Guid ClassId { get; set; }

        public ICollection<CourseModel>? Course { get; set; }
    }
}
