// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class BannerImageModel : BaseModel
    {
        public EnumBannerImageType BannerImageType { get; set; }

        public string? FilePath { get; set; }

        public EnumBannerLink? RouteScreen { get; set; }

        public string? Url { get; set; }
    }
}
