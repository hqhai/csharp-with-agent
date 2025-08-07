namespace Fsel.Sender.Application.Services.ZaloServices.Models
{
    using System.Text.Json.Serialization;

    public class SendSMSByZaloResponseModel
    {
        [JsonPropertyName("messages")]
        public ICollection<SendSMSByZaloResponseMessageModel>? Messages { get; set; }

        [JsonPropertyName("account")]
        public string? Account { get; set; }

        [JsonPropertyName("errorCode")]
        public string? ErrorCode { get; set; }

        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; set; }

        [JsonPropertyName("referentId")]
        public string? ReferentId { get; set; }

        [JsonPropertyName("from")]
        public string? From { get; set; }

        [JsonPropertyName("type")]
        public int Type { get; set; }

        [JsonPropertyName("serviceType")]
        public int ServiceType { get; set; }
    }

    public class SendSMSByZaloResponseMessageModel
    {
        [JsonPropertyName("errorCode")]
        public string? ErrorCode { get; set; }

        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; set; }

        [JsonPropertyName("referentId")]
        public string? ReferentId { get; set; }

        [JsonPropertyName("to")]
        public string? To { get; set; }

        [JsonPropertyName("requestId")]
        public string? RequestId { get; set; }

        [JsonPropertyName("scheduled")]
        public string? Scheduled { get; set; }

        [JsonPropertyName("templateId")]
        public string? TemplateId { get; set; }

        [JsonPropertyName("useUnicode")]
        public int UseUnicode { get; set; }

        [JsonPropertyName("templateData")]
        public object? TemplateData { get; set; }
    }
}
