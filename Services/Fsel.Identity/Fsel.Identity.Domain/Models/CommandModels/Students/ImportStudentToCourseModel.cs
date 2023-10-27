// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Students
{
    using OfficeOpenXml.Attributes;

    public class ImportStudentToCourseModel
    {
        [EpplusTableColumn(Header = "Email")]
        public string? Email { get; set; }

        [EpplusTableColumn(Header = "Code Course")]
        public string? CodeCourse { get; set; }

        [EpplusTableColumn(Header = "Code Package")]
        public string? CodePackage { get; set; }
    }
}
