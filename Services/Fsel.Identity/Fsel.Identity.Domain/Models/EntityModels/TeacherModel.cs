// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class TeacherModel : BaseModel
    {
        public IList<EnumCourseType>? CourseTypes { get; set; }
        public IList<EnumCourseLevel>? CourseLevels { get; set; }

        public int NumberClass { get; set; }
        public string? PassportPath { get; set; }

        public string? UniversityDegreePath { get; set; }

        public string? CertificationPath { get; set; }

        public string? PoliceClearancePath { get; set; }

        public Guid HumanId { get; set; }

        public TeacherBankAccountModel? TeacherBankAccount { get; set; }
        public HumanModel? Human { get; set; }
    }
}
