// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Services.UserServices.Models
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class CsoModel : BaseModel
    {
        public IList<EnumCourseType>? RoleLives { get; set; }
        public IList<EnumCourseLevel>? CourseLevels { get; set; }
        public IList<EnumSubscriptionClass>? SubscriptionClasses { get; set; }
        public string? PassportPath { get; set; }

        public string? UniversityDegreePath { get; set; }

        public string? CertificationPath { get; set; }

        public string? PoliceClearancePath { get; set; }

        public Guid HumanId { get; set; }

        public HumanModel? Human { get; set; }
    }
}
