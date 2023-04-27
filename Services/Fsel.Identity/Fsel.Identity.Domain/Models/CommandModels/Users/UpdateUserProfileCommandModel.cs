// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Users
{
    public class UpdateUserProfileCommandModel
    {
        public Guid? StudentId { get; set; }
        public string? FullName { get; set; }
        public DateTime Birthday { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? Occupation { get; set; }
        public string? School { get; set; }
        public string? PassportPath { get; set; }
        public string? UniversityDegreePath { get; set; }
        public string? CertificationPath { get; set; }
        public string? PoliceClearancePath { get; set; }
    }
}
