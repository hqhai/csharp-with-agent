// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Entities
{
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class InteractionActions : Entity
    {
        public EnumInteractionActionType Type { get; set; }

        public Guid ObjectId { get; set; }

        public Guid UserId { get; set; }
    }
}
