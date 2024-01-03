// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels.UrBox
{
    using System.Text.Json.Serialization;

    public class GetAllGiftModel : UrBoxModel
    {
        [JsonPropertyName("data")]
        public DataGetAllModel? Data { get; set; }
    }

    public class DataGetAllModel
    {
        [JsonPropertyName("items")]
        public IList<GiftModel>? Items { get; set; }

        [JsonPropertyName("totalPage")]
        public long TotalPage { get; set; }

        [JsonPropertyName("totalResult")]
        public string? TotalResult { get; set; }
    }

    public partial class GiftModel
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("brand")]
        public string? Brand { get; set; }

        [JsonPropertyName("brand_id")]
        public string? BrandId { get; set; }

        [JsonPropertyName("cat_id")]
        public string? CatId { get; set; }

        [JsonPropertyName("cat_title")]
        public string? CatTitle { get; set; }

        [JsonPropertyName("gift_id")]
        public string? GiftId { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("price")]
        public string? Price { get; set; }

        [JsonPropertyName("point")]
        public string? Point { get; set; }

        [JsonPropertyName("view")]
        public string? View { get; set; }

        [JsonPropertyName("quantity")]
        public string? Quantity { get; set; }

        [JsonPropertyName("stock")]
        public long Stock { get; set; }

        [JsonPropertyName("image")]
        public string? Image { get; set; }

        [JsonPropertyName("images")]
        public object? Images { get; set; }

        [JsonPropertyName("images_rectangle")]
        public object? ImagesRectangle { get; set; }

        [JsonPropertyName("expire_duration")]
        public string? ExpireDuration { get; set; }

        [JsonPropertyName("code_display")]
        public string? CodeDisplay { get; set; }

        [JsonPropertyName("code_display_type")]
        public long CodeDisplayType { get; set; }

        [JsonPropertyName("price_promo")]
        public long PricePromo { get; set; }

        [JsonPropertyName("start_promo")]
        public long StartPromo { get; set; }

        [JsonPropertyName("end_promo")]
        public long EndPromo { get; set; }

        [JsonPropertyName("is_promo")]
        public long IsPromo { get; set; }

        [JsonPropertyName("is_unfix")]
        public string? IsUnfix { get; set; }

        [JsonPropertyName("office")]
        public IList<OfficeModel>? Offices { get; set; }

        [JsonPropertyName("brandLogoLoyalty")]
        public string? BrandLogoLoyalty { get; set; }

        [JsonPropertyName("brandImage")]
        public string? BrandImage { get; set; }

        [JsonPropertyName("brand_name")]
        public string? BrandName { get; set; }

        [JsonPropertyName("brand_online")]
        public string? BrandOnline { get; set; }

        [JsonPropertyName("parent_cat_id")]
        public string? ParentCatId { get; set; }

        [JsonPropertyName("usage_check")]
        public long UsageCheck { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("note")]
        public string? Note { get; set; }

        [JsonPropertyName("code_quantity")]
        public string? CodeQuantity { get; set; }
    }

    public partial class OfficeModel
    {
        [JsonPropertyName("brand_id")]
        public string? BrandId { get; set; }

        [JsonPropertyName("city_id")]
        public string? CityId { get; set; }

        [JsonPropertyName("district_id")]
        public string? DistrictId { get; set; }

        [JsonPropertyName("ward_id")]
        public string? WardId { get; set; }

        [JsonPropertyName("street_id")]
        public string? StreetId { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("address_en")]
        public string? AddressEn { get; set; }

        [JsonPropertyName("number")]
        public string? Number { get; set; }

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("latitude")]
        public string? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public string? Longitude { get; set; }

        [JsonPropertyName("geo")]
        public string? Geo { get; set; }

        [JsonPropertyName("isApply")]
        public string? IsApply { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("brand_img_src")]
        public string? BrandImgSrc { get; set; }

        [JsonPropertyName("brand_title")]
        public string? BrandTitle { get; set; }

        [JsonPropertyName("title_city")]
        public string? TitleCity { get; set; }
    }
}
