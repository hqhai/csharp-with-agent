// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Core.Entities;

    public class Techie : Entity
    {
        public string? Code { get; set; }

        public string? Name { get; set; }

        public string? Icon { get; set; }

        public string? Image { get; set; }

        public IList<TechieAction>? TechieActions { get; set; }
    }
}
