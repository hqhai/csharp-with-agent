// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Application.Services.SMSServices.IRIS.Models
{
    using System.Text.Json.Serialization;

    public class IRISSMSTokenResponseModel
    {
        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        private int? _expiresIn;

        [JsonPropertyName("expires_in")]
        public int? ExpiresIn
        {
            get => _expiresIn;
            set
            {
                _expiresIn = value;
                ExpiresAt = DateTime.UtcNow.AddSeconds(value ?? default);
            }
        }

        [JsonIgnore]
        public DateTime? ExpiresAt { get; private set; }
    }
}
