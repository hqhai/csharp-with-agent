// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class TechieAction : Entity
    {
        public string? Config { get; set; }

        public string? TemplateMessage { get; set; }

        public int Priority { get; set; }

        public string? Icon { get; set; }

        public string? Image { get; set; }

        public EnumTechieAction Action { get; set; }
        public EnumTechieFeature Feature { get; set; }

        public Guid TechieId { get; set; }

        public Techie Techie { get; set; } = new Techie();
    }
}
