// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Domain.Enums;

    public class UserDeletionModel : BaseModel
    {
        public EnumUserDeletionReason Reason { get; set; }
        public string? ReasonContent { get; set; }
        public DateTime DeletionDate { get; set; }
        public EnumUserDeletionStatus Status { get; set; }
        public Guid UserId { get; set; }
    }
}
