// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.SystemService.Models
{
    using System.Collections.Generic;

    public class AddVouchersForMAIntoGoogleSheetCommandModel
    {
        public IList<string>? Vouchers { get; set; }
        public string? Package { get; set; }
        public string? ExpiredDate { get; set; }
        public string? MACode { get; set; }
    }
}
