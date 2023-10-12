// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Core.Entities;

    public class FocusTimeConfig : Entity
    {
        public double TargetTime { get; set; }

        public string? Description { get; set; }

        public int Token { get; set; }
    }
}
