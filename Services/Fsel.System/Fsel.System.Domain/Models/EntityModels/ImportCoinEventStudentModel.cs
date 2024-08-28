// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using OfficeOpenXml.Attributes;

    public class ImportCoinEventStudentModel
    {
        [EpplusTableColumn(Header = "Email")]
        public string? Email { get; set; }

        [EpplusTableColumn(Header = "Coin")]
        public double Coin { get; set; }

        [EpplusTableColumn(Header = "EventCode")]
        public string? EventCode { get; set; }
    }
}
