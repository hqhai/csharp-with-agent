// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.CommandModels.Flags
{
    public class ApproveFlagCommandModel
    {
        public Guid ObjectId { get; set; }
        public bool IsApprove { get; set; }
    }
}
