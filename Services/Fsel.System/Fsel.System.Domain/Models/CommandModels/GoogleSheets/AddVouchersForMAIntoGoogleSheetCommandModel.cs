// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.GoogleSheets
{
    public class AddVouchersForMAIntoGoogleSheetCommandModel
    {
        public IList<string>? Vouchers { get; set; }
        public string? Package { get; set; }
        public string? ExpiredDate { get; set; }
        public string? MACode { get; set; }
    }
}
