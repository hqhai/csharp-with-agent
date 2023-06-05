// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class InteractionActionModel : BaseModel
    {
        public EnumInteractionActionType Type { get; set; }

        public Guid ObjectId { get; set; }

        public Guid UserId { get; set; }
    }
}
