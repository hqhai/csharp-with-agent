// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Admins
{
    using OfficeOpenXml.Attributes;

    public class PtImportRowCommandModel
    {
        [EpplusTableColumn(Header = "Email")]
        public string? Email { get; set; }

        [EpplusTableColumn(Header = "CourseLevelPt")]
        public string? CourseLevel { get; set; }

        [EpplusTableColumn(Header = "PhoneNumber")]
        public string? PhoneNumber { get; set; }
    }
}
