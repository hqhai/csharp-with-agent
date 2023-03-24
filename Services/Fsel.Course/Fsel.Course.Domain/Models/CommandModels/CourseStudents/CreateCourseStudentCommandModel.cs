// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.CourseStudents
{
    using System;

    public class CreateCourseStudentCommandModel
    {
        public Guid CourseId { get; set; }
        public Guid StudentId { get; set; }
    }
}
