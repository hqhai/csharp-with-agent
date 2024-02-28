// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.QueryModels.StudentTrialRegistration
{
    using Fsel.Shared.Enums;

    public class StudentTrialRegistrationQueryModel
    {
        public Guid StudentId { get; set; }

        public Guid CourseId { get; set; }

    }
}
