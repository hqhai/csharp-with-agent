// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Campus
{
    using OfficeOpenXml.Attributes;

    public class AddStudentsToCurriculumModel
    {
        [EpplusTableColumn(Header = "Họ và tên")]
        public string? FullName { get; set; }

        [EpplusTableColumn(Header = "Email")]
        public string? Email { get; set; }
    }
}
