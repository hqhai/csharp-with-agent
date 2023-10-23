// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.CommandModels.Flags
{
    using System.Collections.Generic;

    public class UpdateStatusFlagsCommandModel
    {
        public IList<UpdateStatusFlagCommandModel>? ListFlag { get; set; }
    }
}
