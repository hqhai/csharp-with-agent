// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class PackageTranslationModel : BaseModel
    {
        public string? IncentivesWhenPurchasing { get; set; }
        public Guid PackageId { get; set; }
        public string? Language { get; set; }
    }
}
