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

        [EpplusTableColumn(Header = "Send Notify")]
        public string? IsSendNotify { get; set; }

        [EpplusTableColumn(Header = "EnumNotify")]
        public string? EnumNotify { get; set; }
    }
}
