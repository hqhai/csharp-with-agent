// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.TrainingService.CommandModels
{
    using System;

    public class AddStudentIntoClassCommandModel
    {
        public Guid UserId { get; set; }
        public Guid CourseId { get; set; }
        public Guid PackageId { get; set; }
    }
}
