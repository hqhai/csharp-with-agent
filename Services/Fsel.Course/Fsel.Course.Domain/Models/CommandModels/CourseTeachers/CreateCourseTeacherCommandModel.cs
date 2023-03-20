// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.CourseTeachers
{
    using System;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using System.ComponentModel.DataAnnotations;

    public class CreateCourseTeacherCommandModel
    {
        public Guid TeacherId { get; set; }

        public string? Nationality { get; set; }

        public string? Deggree { get; set; }

        public string? Experience { get; set; }

        public string? Strength { get; set; }
    }
}
