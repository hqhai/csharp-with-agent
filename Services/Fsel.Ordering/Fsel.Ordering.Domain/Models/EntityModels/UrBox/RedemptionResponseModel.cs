// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels.UrBox
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class RedemptionResponseModel
    {
        [JsonPropertyName("done")]
        public long Done { get; set; }

        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("microtime")]
        public string? MicroTime { get; set; }

        [JsonPropertyName("status")]
        public long Status { get; set; }

        [JsonPropertyName("data")]
        public DataRedemptionRequestModel? Data { get; set; }
    }

    public class DataRedemptionRequestModel
    {
        [JsonPropertyName("pay")]
        public long Pay { get; set; }

        [JsonPropertyName("transaction_id")]
        public string? TransactionId { get; set; }

        [JsonPropertyName("cart_created")]
        public string? CartCreated { get; set; }

        [JsonPropertyName("linkCart")]
        public string? LinkCart { get; set; }

        [JsonPropertyName("linkCombo")]
        public string? LinkCombo { get; set; }

        [JsonPropertyName("linkShippingInfo")]
        public string? LinkShippingInfo { get; set; }

        [JsonPropertyName("cart")]
        public CartRedemptionRequestModel? Cart { get; set; }
    }

    public partial class CartRedemptionRequestModel
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("cartNo")]
        public string? CartNo { get; set; }

        [JsonPropertyName("money_total")]
        public string? MoneyTotal { get; set; }

        [JsonPropertyName("money_ship")]
        public string? MoneyShip { get; set; }

        [JsonPropertyName("link_gift")]
        public IList<string>? LinkGift { get; set; }

        [JsonPropertyName("code_link_gift")]
        public IList<CodeLinkGift>? CodeLinkGift { get; set; }
    }

    public class CodeLinkGift
    {
        [JsonPropertyName("cart_detail_id")]
        public string? CartDetailId { get; set; }

        [JsonPropertyName("code_display")]
        public string? CodeDisplay { get; set; }

        [JsonPropertyName("code_display_type")]
        public long CodeDisplayType { get; set; }

        [JsonPropertyName("link")]
        public string? Link { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("card_id")]
        public long CardId { get; set; }

        [JsonPropertyName("pin")]
        public string? Pin { get; set; }

        [JsonPropertyName("serial")]
        public string? Serial { get; set; }

        [JsonPropertyName("priceId")]
        public string? PriceId { get; set; }

        [JsonPropertyName("gift_id")]
        public string? GiftId { get; set; }

        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("expired")]
        public string? Expired { get; set; }

        [JsonPropertyName("expired_time")]
        public long ExpiredTime { get; set; }

        [JsonPropertyName("code_image")]
        public string? CodeImage { get; set; }

        [JsonPropertyName("estimateDelivery")]
        public string? EstimateDelivery { get; set; }

        [JsonPropertyName("ttemail")]
        public string? Email { get; set; }

        [JsonPropertyName("ttphone")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("receive_code")]
        public string? ReceiveCode { get; set; }

        [JsonPropertyName("city_id")]
        public long CityId { get; set; }

        [JsonPropertyName("district_id")]
        public long DistrictId { get; set; }

        [JsonPropertyName("ward_id")]
        public long WardId { get; set; }

        [JsonPropertyName("delivery_note")]
        public string? DeliveryNote { get; set; }

        [JsonPropertyName("ttaddress")]
        public string? Address { get; set; }

        [JsonPropertyName("deliveryCode")]
        public long DeliveryCode { get; set; }

        [JsonPropertyName("type")]
        public long Type { get; set; }

        [JsonPropertyName("urcard_id")]
        public long UrCardId { get; set; }

        [JsonPropertyName("price")]
        public long Price { get; set; }
    }
}
