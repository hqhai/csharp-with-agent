// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class FeatureAccessTime : Entity
    {
        public EnumFeature EnumFeature { get; set; }
    }
}
