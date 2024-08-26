// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.CommandModels.Classes
{
    public class AddStudentIntoClassCommandModel
    {
        public Guid UserId { get; set; }
        public Guid? PackageId { get; set; }
        public Guid CourseId { get; set; }
        public int NumberOfShield { get; set; }
    }
}
