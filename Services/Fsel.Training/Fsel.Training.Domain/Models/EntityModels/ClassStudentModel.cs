// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.EntityModels
{
    using System;

    public class ClassStudentModel
    {
        public Guid StudentId { get; set; }
        public string? StudentName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Code { get; set; }
        public Guid? PackageId { get; set; }
    }
}
