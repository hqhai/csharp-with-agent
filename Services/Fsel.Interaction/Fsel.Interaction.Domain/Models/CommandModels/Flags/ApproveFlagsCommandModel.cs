// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.CommandModels.Flags
{
    using System.Collections.Generic;

    public class ApproveFlagsCommandModel
    {
        public IList<ApproveFlagCommandModel>? Flags { get; set; }
    }
}
