// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.CourseTeachers
{
    using System;

    public class CreateCourseTeacherCommandModel
    {
        public Guid TeacherId { get; set; }

        public string? AvatarPath { get; set; }

        public string? Nationality { get; set; }

        public string? Deggree { get; set; }

        public string? Experience { get; set; }

        public string? Strength { get; set; }
    }
}
