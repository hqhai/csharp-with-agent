// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using Fsel.Core.Entities;

    public class SystemConfig : Entity
    {
        public string? Type { get; set; }
        public bool IsEnabled { get; set; }
    }
}
