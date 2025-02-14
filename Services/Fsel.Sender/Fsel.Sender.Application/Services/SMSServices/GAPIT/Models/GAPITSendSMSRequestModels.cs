// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Application.Services.SMSServices.GAPIT.Models
{
    using System.Text.Json.Serialization;
    using Refit;

    public class GAPITSendSMSRequestModels
    {
        [AliasAs("brandname")]
        [JsonPropertyName("brandname")]
        public string? BrandName { get; set; }

        [AliasAs("cpid")]
        [JsonPropertyName("cpid")]
        public string? CPId { get; set; }

        [AliasAs("sending_list")]
        [JsonPropertyName("sending_list")]
        public IList<GAPITSendSMSRequestModel> SendingList { get; set; } = new List<GAPITSendSMSRequestModel>();
    }

    public class GAPITSendSMSRequestModel
    {
        [AliasAs("dest")]
        [JsonPropertyName("dest")]
        public string? PhoneNumber { get; set; }

        [AliasAs("msgbody")]
        [JsonPropertyName("msgbody")]
        public string? Content { get; set; }

        [AliasAs("content_type")]
        [JsonPropertyName("content_type")]
        public string? ContentType { get; set; }

        [AliasAs("serviceid")]
        [JsonPropertyName("serviceid")]
        public string? ServiceId { get; set; }

        [AliasAs("mtid")]
        [JsonPropertyName("mtid")]
        public string? MTId { get; set; }
    }
}
