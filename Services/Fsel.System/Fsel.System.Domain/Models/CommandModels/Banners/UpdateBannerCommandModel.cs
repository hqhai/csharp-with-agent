// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.Banners
{
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Models.CommandModels.BannerScopes;
    using global::System.ComponentModel.DataAnnotations;

    public class UpdateBannerCommandModel
    {
        public Guid Id { get; set; }

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
        public EnumBannerLink? RouteScreen { get; set; }

        [Required]
        public IList<UpdateBannerScopeCommandModel> BannerScopes { get; set; } = new List<UpdateBannerScopeCommandModel>();
    }
}
