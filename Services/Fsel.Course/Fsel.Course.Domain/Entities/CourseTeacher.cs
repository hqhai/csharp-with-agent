// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;

    public class CourseTeacher : Entity
    {
        public Course? Course { get; set; }

        public Guid TeacherId { get; set; }

        [Required(ErrorMessage = nameof(EnumCourseErrorCode.C01V))]
        public Guid CourseId { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumCourseTeacherErrorCode.CT03C))]
        public string? Nationality { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumCourseTeacherErrorCode.CT03C))]
        public string? Deggree { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumCourseTeacherErrorCode.CT03C))]
        public string? Experience { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumCourseTeacherErrorCode.CT03C))]
        public string? Strength { get; set; }
    }
}
