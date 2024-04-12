// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Admins
{
    public class CreateUserStudentsToAdminCommandModel
    {
        public IList<CreateUserStudentToAdminCommandModel>? Users { get; set; }
        public Guid CourseId { get; set; }
    }

    public class CreateUserStudentToAdminCommandModel
    {
        public string? Email { get; set; }
        public bool IsTrialRegistration { get; set; }
    }
}
