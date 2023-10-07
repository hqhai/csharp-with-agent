// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Students
{
    using System;

    public class UpdateReferralCodeStudentCommandModel
    {
        public string? ReferralCode { get; set; }
        public Guid UserId { get; set; }
    }
}
