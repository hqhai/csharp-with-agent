// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.SystemService.Models
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class LocationModel : BaseModel
    {
        public int Code { get; set; }
        public int ProvinceCode { get; set; }
        public int DistrictCode { get; set; }
        public EnumLocationType? Type { get; set; }
        public Guid? ParentId { get; set; }
        public string? Name { get; set; }
        public string? LocalId { get; set; }
        public string? LongPath { get; set; }
    }
}
