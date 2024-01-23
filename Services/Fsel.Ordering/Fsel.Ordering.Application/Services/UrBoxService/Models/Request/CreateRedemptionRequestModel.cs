// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.UrBoxService.Models.Request
{
    using System.Text.Json.Serialization;
    using Fsel.Ordering.Infrastructure.ValueSettings;

    public class CreateRedemptionRequestModel : UrBoxSignatureModel
    {
        public CreateRedemptionRequestModel(AppSetting appSetting) : base(appSetting)
        {
        }

        [JsonPropertyName("ttphone")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("lang")]
        public string? Language { get; set; }

        [JsonPropertyName("ttemail")]
        public string? Email { get; set; }

        [JsonPropertyName("shipping_info_available")]
        public long? ShippingInfoAvailable { get; set; }

        [JsonPropertyName("city_id")]
        public string? CityId { get; set; }

        [JsonPropertyName("district_id")]
        public string? DistrictId { get; set; }

        [JsonPropertyName("ward_id")]
        public string? WardId { get; set; }

        [JsonPropertyName("ttaddress")]
        public string? TtAddress { get; set; }

        [JsonPropertyName("delivery_note")]
        public string? DeliveryNote { get; set; }
    }
}
