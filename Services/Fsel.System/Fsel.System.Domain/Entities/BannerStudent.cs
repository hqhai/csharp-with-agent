// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Core.Entities;

    public class BannerStudent : Entity
    {
        public Guid StudentId { get; set; }
        public Banner? Banner { get; set; }
        public Guid BannerId { get; set; }
    }
}
