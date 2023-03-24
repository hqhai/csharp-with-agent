// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class CourseTeacherModel : BaseModel
    {
        public TeacherModel? Teacher { get; set; }
        public Guid TeacherId { get; set; }
        public Guid CourseId { get; set; }
    }

    public class TeacherModel : BaseModel
    {
        public string? PassportPath { get; set; }

        public string? UniversityDegreePath { get; set; }

        public string? CertificationPath { get; set; }

        public string? PoliceClearancePath { get; set; }

        public Guid HumanId { get; set; }

        public HumanModel? Human { get; set; }
    }

    public class HumanModel
    {
        public string? FullName { get; set; }
    }
}
