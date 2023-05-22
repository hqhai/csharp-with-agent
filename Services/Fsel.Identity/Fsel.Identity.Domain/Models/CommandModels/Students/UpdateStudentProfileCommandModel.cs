// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Students
{
    public class UpdateStudentProfileCommandModel
    {
        public Guid? StudentId { get; set; }
        public string? AvatarPath { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Address { get; set; }
        public string? Occupation { get; set; }
        public string? School { get; set; }
    }
}
