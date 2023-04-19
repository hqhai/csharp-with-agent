// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.UserServices.Models
{
    using System;
    using Fsel.Shared.Enums;
    using Fsel.Core.Base.BaseModels;

    public class StudentModel : BaseModel
    {
        public string? Membership { get; set; }

        public string? Occupation { get; set; }

        public string? School { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public Guid? ClassId { get; set; }

        public HumanModel? Human { get; set; }
    }
}
