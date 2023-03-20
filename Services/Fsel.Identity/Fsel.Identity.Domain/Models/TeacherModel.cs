// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Domain.Models.EntityModels;

    public class TeacherModel : BaseEntityModel
    {
        public string? PassportPath { get; set; }

        public string? UniversityDegreePath { get; set; }

        public string? CertificationPath { get; set; }

        public string? PoliceClearancePath { get; set; }

        public Guid HumanId { get; set; }
        public HumanModel? Human { get; set; }
    }
}
