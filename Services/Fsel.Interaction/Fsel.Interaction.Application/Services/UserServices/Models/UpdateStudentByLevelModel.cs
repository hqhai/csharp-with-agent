// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.UserServices.Models
{
    using System;
    using Fsel.Shared.Enums;

    public class UpdateStudentByLevelModel
    {
        public Guid Id { get; set; }
        public EnumCourseLevel Level { get; set; }
    }
}
