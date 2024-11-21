// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class BannerStudentModel
    {
        public Guid BannerId { get; set; }

        public string? Name { get; set; }

        public string? Url { get; set; }

        public string? FilePath { get; set; }

        public string? Content { get; set; }

        public EnumBannerType Type { get; set; }
    }
}
