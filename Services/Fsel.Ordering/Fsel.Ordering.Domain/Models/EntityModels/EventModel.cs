// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels
{
    using System;
    using System.Collections.Generic;
    using Fsel.Common.Enums.ErrorCodes;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class EventModel : BaseModel
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Description { get; set; }
        public IList<string>? ImagePaths { get; set; }
        public EnumEventPackageStatus Status { get; set; }
    }

    public class PackageEventModel : BaseModel
    {
        public decimal Price { get; set; }
        public decimal PriceMonth { get; set; }
        public int DayBonus { get; set; }
        public int MonthBonus { get; set; }
        public EnumPackageSuggest? Suggest { get; set; }
        public bool IsDefault { get; set; }
    }
}
