// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.GoogleSheets
{
    public class AddPaymentInfoToGoogleSheetCommandModel
    {
        public string? Code { get; set; }
        public string? CreatedDate { get; set; }
        public string? Price { get; set; }
        public string? FullName { get; set; }
        public string? StudentEmail { get; set; }
        public string? BillingEmail { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyAddress { get; set; }
        public string? CompanyTaxCode { get; set; }
        public string? CompanyEmail { get; set; }
    }
}
