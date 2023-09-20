// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Fsel.Shared.Enums;

namespace Fsel.Identity.Domain.Models.CommandModels.Users
{
    public class CreateUserCommandModel
    {
        public string? AvatarPath { get; set; }

        [Required]
        public string? FullName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? Address { get; set; }

        [Required]
        public DateTime Birthday { get; set; }

        public string? PhoneNumber { get; set; }
        public IList<EnumCourseType>? LiveCourseTypes { get; set; }
        public IList<EnumCourseType>? CourseTypes { get; set; }
        public IList<EnumCourseLevel>? CourseLevels { get; set; }
        public IList<Guid>? PackageIds { get; set; }

        [Required]
        public EnumRoleRegisterWithAdmin Role { get; set; }

        public string? BankAccountName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankName { get; set; }
    }
}
