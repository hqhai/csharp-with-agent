// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;

    public class CourseClassStudentModel
    {
        public Guid CourseId { get; set; }
        public Guid StudentId { get; set; }
        public Guid ClassId { get; set; }
    }
}
