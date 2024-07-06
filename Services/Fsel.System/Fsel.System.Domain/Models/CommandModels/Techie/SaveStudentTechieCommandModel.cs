// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.Techie
{
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;

    public class SaveStudentTechieCommandModel
    {
        public TechieConfig? Config { get; set; }

        public EnumTechieAction Actions { get; set; }
        public EnumTechieFeature TechieFeature { get; set; }
    }
}
