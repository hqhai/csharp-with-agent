namespace Fsel.Shared.Models.ShareModels
{
    using System.Text.Json.Serialization;

    public class MenuConfig
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("code_title")]
        public string? CodeTitle { get; set; }

        [JsonPropertyName("link")]
        public string? Link { get; set; }

        [JsonPropertyName("icon")]
        public string? Icon { get; set; }

        [JsonPropertyName("parentId")]
        public int? ParentId { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("permission")]
        public string? Permission { get; set; }

        [JsonPropertyName("children")]
        public IList<MenuConfig>? Children { get; set; }
    }
}
