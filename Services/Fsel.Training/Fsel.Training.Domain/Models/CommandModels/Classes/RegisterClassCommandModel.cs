// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.CommandModels.Classes
{
    using Fsel.Shared.Enums;

    public class RegisterClassCommandModel
    {
        public Guid CourseId { get; set; }
        public string? Code { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
    }
}
