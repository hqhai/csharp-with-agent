// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Application.Services.SMSServices.GAPIT.Models
{
    using System.Text.Json.Serialization;

    public class GAPITSendSMSResponseModels
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("result")]
        public IList<GAPITSendSMSResponseModel>? Result { get; set; }
    }

    public class GAPITSendSMSResponseModel
    {
        [JsonPropertyName("mtid")]
        public string? MTId { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("reqid")]
        public string? RequestId { get; set; }

        [JsonPropertyName("telco")]
        public string? Telco { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
