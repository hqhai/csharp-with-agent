// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels
{
    using System;
    using System.Collections.Generic;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class EventModel : BaseModel
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
        public IList<string>? ImagePaths { get; set; }
        public EnumEventPackageStatus Status { get; set; }
        public bool EventStatus { get; set; }
        public IList<PackageEventModel>? PackageEvents { get; set; }
        public IList<EventTranslationModel>? Translations { get; set; }
    }

    public class PackageEventModel
    {
        public Guid? Id { get; set; }
        public decimal Price { get; set; }
        public Guid PackageId { get; set; }
        public decimal PriceMonth { get; set; }
        public int DayBonus { get; set; }
        public int MonthBonus { get; set; }
        public int Month { get; set; }
        public IList<EnumPackageSuggest>? Suggests { get; set; }
    }

    public class EventTranslationModel : BaseModel
    {
        public string? Description { get; set; }

        public Guid EventId { get; set; }

        public string? Language { get; set; }
    }
}
