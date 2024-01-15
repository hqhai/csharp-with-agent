// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.UrBoxService.Models.Response
{
    using System.Text.Json.Serialization;

    public class GetGiftDetailModel : UrBoxModel
    {
        [JsonPropertyName("data")]
        public GiftDetailModel? Data { get; set; }
    }

    public class GiftDetailModel
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("brand")]
        public string? Brand { get; set; }

        [JsonPropertyName("brand_id")]
        public string? BrandId { get; set; }

        [JsonPropertyName("code_display")]
        public string? CodeDisplay { get; set; }

        [JsonPropertyName("code_display_type")]
        public long CodeDisplayType { get; set; }

        [JsonPropertyName("cat_id")]
        public string? CatId { get; set; }

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

        [JsonPropertyName("valuex")]
        public string? ValueX { get; set; }

        [JsonPropertyName("weight")]
        public string? Weight { get; set; }

        [JsonPropertyName("justGetOrder")]
        public string? JustGetOrder { get; set; }

        [JsonPropertyName("view")]
        public string? View { get; set; }

        [JsonPropertyName("quantity")]
        public string? Quantity { get; set; }

        [JsonPropertyName("usage_check")]
        public long UsageCheck { get; set; }

        [JsonPropertyName("image")]
        public string? Image { get; set; }

        [JsonPropertyName("images")]
        public object? Images { get; set; }

        [JsonPropertyName("images_rectangle")]
        public IList<object>? ImagesRectangle { get; set; }

        [JsonPropertyName("expire_duration")]
        public string? ExpireDuration { get; set; }

        [JsonPropertyName("parent_cat_id")]
        public string? ParentCatId { get; set; }

        [JsonPropertyName("brand_online")]
        public string? BrandOnline { get; set; }

        [JsonPropertyName("brandImage")]
        public string? BrandImage { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("note")]
        public string? Note { get; set; }

        [JsonPropertyName("office")]
        public IList<OfficeDetailModel>? Offices { get; set; }
    }

    public class OfficeDetailModel
    {
        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("address_en")]
        public string? AddressEn { get; set; }

        [JsonPropertyName("city_id")]
        public string? CityId { get; set; }

        [JsonPropertyName("latitude")]
        public string? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public string? Longitude { get; set; }

        [JsonPropertyName("brand_id")]
        public string? BrandId { get; set; }

        [JsonPropertyName("district_id")]
        public string? DistrictId { get; set; }

        [JsonPropertyName("ward_id")]
        public string? WardId { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("number")]
        public string? Number { get; set; }

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("geo")]
        public string? Geo { get; set; }

        [JsonPropertyName("isApply")]
        public string? IsApply { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("title_city")]
        public string? TitleCity { get; set; }
    }
}
