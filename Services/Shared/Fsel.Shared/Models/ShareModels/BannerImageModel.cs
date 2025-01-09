// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using System.Text.Json.Serialization;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class BannerImageModel : BaseModel
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EnumBannerImageType BannerImageType { get; set; }

        public string? FilePath { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EnumBannerLink? RouteScreen { get; set; }

        public string? Url { get; set; }
    }
}
