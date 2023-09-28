// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.UserServices.Models
{
    using System;
    using System.Collections.Generic;
    using Fsel.Shared.Enums;

    public class CsoModel
    {
        public IList<EnumCourseType>? CourseTypes { get; set; }
        public IList<EnumCourseLevel>? CourseLevels { get; set; }
        public IList<Guid>? PackageIds { get; set; }
        public string? PassportPath { get; set; }

        public string? UniversityDegreePath { get; set; }

        public string? CertificationPath { get; set; }

        public string? PoliceClearancePath { get; set; }

        public Guid HumanId { get; set; }

        public HumanModel? Human { get; set; }
    }
}
