// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.CommandModels.Actions
{
    using Fsel.Shared.Enums;

    public class CreateActionCommandModel
    {
        public Guid ObjectId { get; set; }
        public EnumInteractionActionType Type { get; set; }
        public EnumInteractionType BusinessType { get; set; }
    }
}
