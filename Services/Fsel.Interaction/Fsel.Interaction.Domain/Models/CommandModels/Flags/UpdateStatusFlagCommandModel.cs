// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.CommandModels.Flags
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Interaction.Domain.Enums;

    public class UpdateStatusFlagCommandModel : BaseCommandModel
    {
        public EnumFlagStatus Status { get; set; }
    }
}
