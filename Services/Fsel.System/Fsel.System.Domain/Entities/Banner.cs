// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Common.Attributes;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using global::System.ComponentModel.DataAnnotations;

    public class Banner : Entity
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Code { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Url { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? FilePath { get; set; }

        [MaxLength(3000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Content { get; set; }

        public EnumBannerType Type { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public EnumBannerFrequency BannerFrequency { get; set; }

        //[RequiredIf(nameof(BannerFrequency), EnumBannerFrequency.Custom, ErrorMessage = nameof(EnumBannerErrorCode.CustomFrequencyRequiresDisplayDate))]
        public DateTime? DisplayStartDate { get; set; }

        //[RequiredIf(nameof(BannerFrequency), EnumBannerFrequency.Custom, ErrorMessage = nameof(EnumBannerErrorCode.CustomFrequencyRequiresDisplayDate))]
        public DateTime? DisplayEndDate { get; set; }

        [Range(0, long.MaxValue, ErrorMessage = nameof(EnumBannerErrorCode.DisplayStartTimeMustGreaterThanZero))]
        public long? DisplayStartTime { get; set; }

        [Range(0, long.MaxValue, ErrorMessage = nameof(EnumBannerErrorCode.DisplayStartTimeMustGreaterThanZero))]
        public long? DisplayEndTime { get; set; }

        public bool Status { get; set; }
        public EnumBannerLink? RouteScreen { get; set; }

        public ICollection<BannerScope> BannerScopes { get; set; } = new List<BannerScope>();

        public ICollection<BannerStudent> BannerStudents { get; set; } = new List<BannerStudent>();
    }
}
