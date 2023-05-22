// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.UserServices.Models
{
    using Fsel.Shared.Enums;

    public class UpdateStudentByLevelModel
    {
        public Guid Id { get; set; }
        public EnumCourseLevel Level { get; set; }
    }
}
