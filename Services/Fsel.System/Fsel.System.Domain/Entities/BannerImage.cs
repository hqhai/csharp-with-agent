// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using global::System.ComponentModel.DataAnnotations;

    public class BannerImage : Entity
    {
        public EnumBannerImageType BannerImageType { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(1500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? FilePath { get; set; }

        public EnumBannerLink? RouteScreen { get; set; }

        [MaxLength(1500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Url { get; set; }

        public Guid BannerId { get; set; }

        public Banner? Banner { get; set; }
    }
}
