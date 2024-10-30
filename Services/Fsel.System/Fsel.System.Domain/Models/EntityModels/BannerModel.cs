// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class BannerModel : BaseModel
    {
        public string? Code { get; set; }

        public string? Name { get; set; }

        public string? Url { get; set; }

        public string? FilePath { get; set; }

        public string? Content { get; set; }

        public EnumBannerType Type { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public EnumBannerFrequency BannerFrequency { get; set; }

        public DateTime? DisplayStartDate { get; set; }

        public DateTime? DisplayEndDate { get; set; }

        public long? DisplayStartTime { get; set; }

        public long? DisplayEndTime { get; set; }

        public bool Status { get; set; }

        public IList<BannerScopeModel>? BannerScopes { get; set; }
    }
}
