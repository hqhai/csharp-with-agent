namespace Fsel.Ordering.Domain.Models.EntityModels
{
    using System.Text.Json.Serialization;

    public partial class BrandModel
    {
        [JsonPropertyName("done")]
        public long Done { get; set; }

        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("microtime")]
        public string? Microtime { get; set; }

        [JsonPropertyName("status")]
        public long Status { get; set; }

        [JsonPropertyName("data")]
        public DataBrand? Data { get; set; }
    }

    public class DataBrand
    {
        [JsonPropertyName("items")]
        public IList<ItemBrand>? Items { get; set; }

        [JsonPropertyName("textTitle")]
        public string? TextTitle { get; set; }

        [JsonPropertyName("brand_count")]
        public string? BrandCount { get; set; }

        [JsonPropertyName("totalPage")]
        public long TotalPage { get; set; }
    }

    public partial class ItemBrand
    {
        [JsonPropertyName("banner")]
        public string? Banner { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("images")]
        public string? Images { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("cat_id")]
        public string? CatId { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("gift_count")]
        public long GiftCount { get; set; }

        [JsonPropertyName("cat_title")]
        public string? CatTitle { get; set; }

        [JsonPropertyName("parent_cat_id")]
        public string? ParentCatId { get; set; }
    }
}
