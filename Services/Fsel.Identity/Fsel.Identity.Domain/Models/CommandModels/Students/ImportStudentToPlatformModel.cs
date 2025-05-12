// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Students
{
    using OfficeOpenXml.Attributes;

    public class ImportStudentToPlatformModel
    {
        [EpplusTableColumn(Header = "Full Name")]
        public string? FullName { get; set; }

        [EpplusTableColumn(Header = "Email")]
        public string? Email { get; set; }

        [EpplusTableColumn(Header = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [EpplusTableColumn(Header = "Date Of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [EpplusTableColumn(Header = "School")]
        public string? School { get; set; }

        [EpplusTableColumn(Header = "Grade")]
        public string? SchoolGrade { get; set; }

        [EpplusTableColumn(Header = "Class")]
        public string? SchoolClass { get; set; }

        [EpplusTableColumn(Header = "Faculty")]
        public string? SchoolFaculty { get; set; }
    }
}
