// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Application.Services.SMSServices.IRIS.Models
{
    using System.Text.Json.Serialization;
    using Refit;

    public class IRISSMSTokenRequestModel
    {
        [AliasAs("grant_type")]
        [JsonPropertyName("grant_type")]
        public string? GrantType { get; set; }
    }
}
