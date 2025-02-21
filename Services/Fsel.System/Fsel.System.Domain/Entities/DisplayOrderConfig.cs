// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class DisplayOrderConfig : Entity
    {
        public int DisplayOrder { get; set; }

        public EnumDisplayOrder Name { get; set; }

        public bool Status { get; set; }
    }
}
