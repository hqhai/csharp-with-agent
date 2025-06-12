namespace Fsel.Sender.Application.Services.ZaloServices.Models
{
    using Refit;
    using System.Text.Json.Serialization;

    public class SendSMSByZaloRequestModel
    {
        [AliasAs("from")]
        [JsonPropertyName("from")]
        public string? From { get; set; }

        [AliasAs("type")]
        [JsonPropertyName("type")]
        public int Type { get; set; }

        [AliasAs("serviceType")]
        [JsonPropertyName("serviceType")]
        public int ServiceType { get; set; }

        [AliasAs("messages")]
        [JsonPropertyName("messages")]
        public ICollection<SendSMSByZaloRequestMessageModel>? Messages { get; set; }
    }

    public class SendSMSByZaloRequestMessageModel
    {
        [AliasAs("to")]
        [JsonPropertyName("to")]
        public string? To { get; set; }

        [AliasAs("requestID")]
        [JsonPropertyName("requestID")]
        public string? RequestID { get; set; }

        [AliasAs("scheduled")]
        [JsonPropertyName("scheduled")]
        public string? Scheduled { get; set; }

        [AliasAs("templateId")]
        [JsonPropertyName("templateId")]
        public string? TemplateId { get; set; }

        [AliasAs("templateData")]
        [JsonPropertyName("templateData")]
        public object? TemplateData { get; set; }

        [AliasAs("useUnicode")]
        [JsonPropertyName("useUnicode")]
        public int UseUnicode { get; set; }
    }
}
