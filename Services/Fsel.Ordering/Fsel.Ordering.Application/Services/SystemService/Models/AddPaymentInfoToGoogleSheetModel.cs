// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.SystemService.Models
{
    public class AddPaymentInfoToGoogleSheetModel
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
    }
}
