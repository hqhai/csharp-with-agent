// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.TrainingService.CommandModels
{
    using System;
    using Fsel.Ordering.Domain.Enums;
    using Fsel.Shared.Enums;

    public class RegisterClassCommandModel
    {
        public Guid PackageId { get; set; }
        public Guid CourseId { get; set; }
        public string? Code { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
    }
}
