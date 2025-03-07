// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.Banners
{
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Models.CommandModels.BannerImages;

    public class UpdateBannerCommandModel
    {
        public Guid Id { get; set; }

        public string? Code { get; set; }

        public string? Name { get; set; }

        public string? Content { get; set; }

        public EnumBannerType Type { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public EnumBannerFrequency BannerFrequency { get; set; }

        public IList<DateTime>? DisplayDates { get; set; }

        public long? DisplayStartTime { get; set; }

        public long? DisplayEndTime { get; set; }

        public bool Status { get; set; }

        public bool MixPanel { get; set; }

        public IList<CreateBannerCommandModel>? BannerScopes { get; set; }

        public IList<CreateBannerImageCommandModel>? BannerImages { get; set; }
    }
}
