// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.CommandModels.Flags
{
    using Fsel.Shared.Enums;

    public class UpdateStatusFlagCommandModel
    {
        public Guid ObjectId { get; set; }
        public EnumFlagStatus Status { get; set; }
    }
}
