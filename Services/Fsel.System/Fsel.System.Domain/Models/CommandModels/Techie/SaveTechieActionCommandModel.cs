// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.Techie
{
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;

    public class SaveTechieActionCommandModel
    {
        public string? TemplateMessage { get; set; }

        public int Priority { get; set; }

        public string? Icon { get; set; }

        public string? Image { get; set; }

        public EnumTechieAction Action { get; set; }
        public EnumTechieFeature Feature { get; set; }

        public Guid TechieId { get; set; }

        public TechieConfig Config { get; set; } = new TechieConfig();
    }
}
