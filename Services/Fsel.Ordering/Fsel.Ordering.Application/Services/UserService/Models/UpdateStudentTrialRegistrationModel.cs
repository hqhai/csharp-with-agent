// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.UserService.Models
{
    using System;
    using Fsel.Shared.Enums;

    public class UpdateStudentTrialRegistrationModel
    {
        public Guid UserId { get; set; }

        public EnumTrialRegistrationStatus Status { get; set; }
    }
}
