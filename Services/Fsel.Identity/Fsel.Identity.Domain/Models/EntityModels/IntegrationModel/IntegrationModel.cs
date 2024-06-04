// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels.IntegrationModel
{
    using Fsel.Identity.Domain.Enums;

    public class IntegrationModel
    {
        public Guid UserId { get; set; }

        public string? FullName { get; set; }

        public string? UserName { get; set; }

        public string? StudentEmail { get; set; }

        public string? StudentPhone { get; set; }

        public EnumGender? Gender { get; set; }

        public DateTime? Birthday { get; set; }

        public string? Address { get; set; }

        public string? ParentName { get; set; }

        public string? ParentPhone { get; set; }

        public string? ParentEmail { get; set; }

        public EnumGender? ParentGender { get; set; }

        public Guid? SchoolId { get; set; }

        public string? LongPathSchool { get; set; }

        public string? LongPathLocation { get; set; }

        public DateTime? LastDate { get; set; }

        public string? PTLevel { get; set; }

    }
}
