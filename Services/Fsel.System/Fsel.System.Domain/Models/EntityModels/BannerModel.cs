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
        public EnumBannerType Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Description { get; set; }
    }
}
