// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.UserService.Models
{
    using System;
    using Fsel.Shared.Enums;

    public class UpdateStudentByClassIdModel
    {
        public Guid? ClassId { get; set; }
        public Guid StudentId { get; set; }
        public Guid? PackageId { get; set; }
        public int NumberOfShield { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
    }
}
