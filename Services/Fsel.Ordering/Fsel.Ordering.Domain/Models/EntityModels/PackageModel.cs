// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels
{
    using Fsel.Ordering.Domain.Entities.PackageConfigs;
    using Fsel.Shared.Enums;

    public class PackageModel
    {
        public Guid Id { get; set; }
        public EnumPackageCode? Code { get; set; }
        public double Price { get; set; }
        public IList<PackageConfig>? Description { get; set; }
    }
}
