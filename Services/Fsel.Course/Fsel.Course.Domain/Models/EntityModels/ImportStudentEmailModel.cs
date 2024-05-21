// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using OfficeOpenXml.Attributes;

    public class ImportStudentEmailModel
    {
        [EpplusTableColumn(Header = "Email")]
        public string? Email { get; set; }
    }
}
