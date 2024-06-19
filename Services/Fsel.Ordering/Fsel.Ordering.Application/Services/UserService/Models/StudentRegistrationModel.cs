// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.UserService.Models
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class StudentRegistrationModel : BaseModel
    {
        public Guid StudentId { get; set; }

        public EnumTrialRegistrationStatus Status { get; set; }
    }
}
