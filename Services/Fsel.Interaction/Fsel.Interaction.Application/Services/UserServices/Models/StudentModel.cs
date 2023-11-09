// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.UserServices.Models
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class StudentModel : BaseModel
    {
        public Guid PackageId { get; set; }
        public string? Membership { get; set; }

        public string? Occupation { get; set; }

        public string? School { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public Guid? ClassId { get; set; }

        public HumanModel? Human { get; set; }
    }
}
