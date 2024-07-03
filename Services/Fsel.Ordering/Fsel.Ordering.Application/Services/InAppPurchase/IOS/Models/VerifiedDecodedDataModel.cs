// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchase.IOS.Models
{
    public class VerifiedDecodedDataModel<TDataModel>
    {
        public TDataModel? DecodedPayload { get; set; }

        public bool IsValid { get; set; }
    }
}
