// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.BannerImages
{
    using Fsel.Shared.Enums;

    public class CreateBannerImageCommandModel
    {
        public EnumBannerImageType BannerImageType { get; set; }

        public string? FilePath { get; set; }

        public EnumBannerLink? RouteScreen { get; set; }

        public string? Url { get; set; }
    }
}
