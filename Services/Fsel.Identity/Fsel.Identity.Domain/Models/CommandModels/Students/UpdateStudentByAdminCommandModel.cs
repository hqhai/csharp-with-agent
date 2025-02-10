// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Students
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Domain.Models.CommandModels.Parents;

    public class UpdateStudentByAdminCommandModel : BaseCommandModel
    {
        public string? FullName { get; set; }
        public DateTime Birthday { get; set; }
        public string? School { get; set; }
        public string? Address { get; set; }
        public string? Occupation { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? SchoolGrade { get; set; }
        public string? SchoolClass { get; set; }
        public UpdateParentCommandModel? Parent { get; set; }
    }
}
