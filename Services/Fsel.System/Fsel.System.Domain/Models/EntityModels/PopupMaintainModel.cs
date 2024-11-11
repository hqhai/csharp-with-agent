// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using global::System;

    public class PopupMaintainModel : BaseModel
    {
        public string? Name { get; set; }
        public string? Content { get; set; }
        public EnumBannerType Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
