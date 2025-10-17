// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.LandingPages
{
    public class StudentRegisterFormToGoogleSheetCommandModel
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public string? ParentEmail { get; set; }

        public string? ParentPhoneNumber { get; set; }

        public string? District { get; set; }
        public Guid? DistrictId { get; set; }

        public string? School { get; set; }
        public Guid? SchoolId { get; set; }

        public string? SchoolGrade { get; set; }

        public string? SchoolClass { get; set; }
    }
}
