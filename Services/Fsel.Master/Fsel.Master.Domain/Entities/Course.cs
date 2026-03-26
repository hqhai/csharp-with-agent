// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    [Table("Dim_Course", Schema = "dbo")]
    public class Course
    {
        public Guid CourseId { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public Guid LevelId { get; set; }
        public Guid ProgramId { get; set; }
        public Guid SubjectId { get; set; }
        public int UnitTotal { get; set; }
        public int LessonTotal { get; set; }
        public int TestTotal { get; set; }
    }
}