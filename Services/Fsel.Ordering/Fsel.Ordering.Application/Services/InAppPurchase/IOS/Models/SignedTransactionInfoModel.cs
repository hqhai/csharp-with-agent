// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchase.IOS.Models
{
    using Newtonsoft.Json;

    public class SignedTransactionInfoModel
    {
        [JsonProperty("signedTransactionInfo")]
        public string? SignedTransactionInfo { get; set; }
    }
}
