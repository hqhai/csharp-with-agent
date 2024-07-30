// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using System.Text.Json.Serialization;
    using Fsel.Shared.Enums;

    public class OrderQueueModel
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EnumOrderStatus Status { get; set; }
    }
}
