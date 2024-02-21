// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.UserServices.Models
{
    using Fsel.Shared.Enums;

    public class StudentTrialRegistrationModel
    {
        public Guid StudentId { get; set; }

        public EnumTrialRegistrationStatus Status { get; set; }
    }
}
