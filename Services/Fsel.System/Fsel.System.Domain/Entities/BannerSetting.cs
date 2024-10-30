// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Core.Entities;

    public class BannerSetting : Entity
    {
        public int MaximumPerDay { get; set; }

        public long DisplayIntervalTime { get; set; }
    }
}
