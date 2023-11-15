// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.UserServices.Models
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class StudentModel : BaseModel
    {
        public EnumCourseLevel CourseLevel { get; set; }

        public Guid? ClassId { get; set; }

        public HumanProfileModel? Human { get; set; }
    }
}
