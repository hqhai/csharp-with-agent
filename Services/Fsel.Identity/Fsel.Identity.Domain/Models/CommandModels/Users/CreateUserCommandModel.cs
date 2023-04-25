// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Fsel.Shared.Enums;

namespace Fsel.Identity.Domain.Models.CommandModels.Users
{
    public class CreateUserCommandModel
    {
        [Required]
        public string? FullName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }
        public IList<EnumCourseType>? Types { get; set; }
        public IList<EnumCourseLevel>? CourseLevels { get; set; }

        [Required]
        public EnumRoleRegisterWithAdmin Role { get; set; }

        public string? BankAccountName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankName { get; set; }
    }
}
