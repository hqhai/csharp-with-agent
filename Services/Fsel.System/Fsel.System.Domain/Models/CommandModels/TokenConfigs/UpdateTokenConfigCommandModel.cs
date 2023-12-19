// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.TokenConfigs
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class UpdateTokenConfigCommandModel : BaseCommandModel
    {
        public EnumTokenFeature Feature { get; set; }

        public EnumTokenMission Mission { get; set; }

        public object? Config { get; set; }

        public object? SuperConfig { get; set; }
    }
}
