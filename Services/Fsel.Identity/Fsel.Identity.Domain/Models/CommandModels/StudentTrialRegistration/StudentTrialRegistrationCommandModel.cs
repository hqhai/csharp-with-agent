// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.StudentTrialRegistration
{
    using Fsel.Shared.Enums;

    public class StudentTrialRegistrationCommandModel
    {
        public Guid UserId { get; set; }

        public EnumTrialRegistrationStatus Status { get; set; }

    }
}
