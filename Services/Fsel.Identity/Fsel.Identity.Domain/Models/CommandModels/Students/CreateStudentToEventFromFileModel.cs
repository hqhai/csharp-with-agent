// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Students
{
    using OfficeOpenXml.Attributes;

    public class CreateStudentToEventFromFileModel
    {
        [EpplusTableColumn(Header = "Full Name")]
        public string? FullName { get; set; }

        [EpplusTableColumn(Header = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [EpplusTableColumn(Header = "Email")]
        public string? Email { get; set; }

        [EpplusTableColumn(Header = "Date Of Birth")]
        public string? DateOfBirth { get; set; }

        [EpplusTableColumn(Header = "Grade")]
        public string? SchoolGrade { get; set; }

        [EpplusTableColumn(Header = "Class")]
        public string? SchoolClass { get; set; }

        [EpplusTableColumn(Header = "Error Message")]
        public string? ErrorMessage { get; set; }
    }
}
