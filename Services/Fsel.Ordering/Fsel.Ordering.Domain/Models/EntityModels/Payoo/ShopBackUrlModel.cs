// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels.Payoo
{
    using System.Text.Json.Serialization;
    using Refit;

    public class ShopBackUrlModel
    {
        [JsonPropertyName("session")]
        [AliasAs("session")]
        public string? Session { get; set; }
        [JsonPropertyName("order_no")]
        [AliasAs("order_no")]
        public string? OrderNo { get; set; }
        [JsonPropertyName("status")]
        [AliasAs("status")]
        public string? Status { get; set; }
        [JsonPropertyName("errorcode")]
        [AliasAs("errorcode")]
        public int? ErrorCode { get; set; }
        [JsonPropertyName("errormsg")]
        [AliasAs("errormsg")]
        public string? ErrorMsg { get; set; }
        [JsonPropertyName("checksum")]
        [AliasAs("checksum")]
        public string? CheckSum { get; set; }
    }
}
