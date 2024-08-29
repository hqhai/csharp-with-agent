// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.UserDeletions
{
    using Fsel.Identity.Domain.Enums;

    public class CreateUserDeletionCommandModel
    {
        public EnumUserDeletionReason Reason { get; set; }
        public string? ReasonContent { get; set; }
        public DateTime DeletionDate { get; set; }
        public EnumUserDeletionStatus Status { get; set; }
    }
}
