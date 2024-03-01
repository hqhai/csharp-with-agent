// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchase.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public enum ExpirationIntent
    {
        Canceled = 1,
        BillingError = 2,
        NewPriceRefused = 3,
        NotPurchasable = 4,
        Unknown = 5
    }
}
