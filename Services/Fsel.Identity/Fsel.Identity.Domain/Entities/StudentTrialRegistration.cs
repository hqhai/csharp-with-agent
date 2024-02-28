// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class StudentTrialRegistration : Entity
    {
        public Guid UserId { get; set; }

        public EnumTrialRegistrationStatus Status { get; set; }

    }
}
