// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    public class IntegrationUserModel
    {
        public Guid? UserId { get; set; }
        public string? FullName { get; set; }
        public string? UserName { get; set; }
        public string? StudentEmail { get; set; }
        public string? StudentPhone { get; set; }
        public string? Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Address { get; set; }
        public string? ParentName { get; set; }
        public string? ParentPhone { get; set; }
        public string? ParentEmail { get; set; }
        public string? ParentGender { get; set; }
    }
}
