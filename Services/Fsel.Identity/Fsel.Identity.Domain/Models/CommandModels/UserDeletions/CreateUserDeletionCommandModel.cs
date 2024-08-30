// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.UserDeletions
{
    using Fsel.Identity.Domain.Enums;

    public class CreateUserDeletionCommandModel
    {
        public string? Password { get; set; }
        public EnumUserDeletionReason Reason { get; set; }
        public string? ReasonContent { get; set; }
    }
}
