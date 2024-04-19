// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Admins
{
    public class CreateUserStudentsToAdminCommandModel
    {
        public IList<string>? Emails { get; set; }
        public bool IsTrialRegistration { get; set; } = false;
        public Guid CourseId { get; set; }
    }
}
