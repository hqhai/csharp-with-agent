// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Users
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class UpdateUserCommandModel : BaseCommandModel
    {
        [Required]
        public string? FullName { get; set; }

        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public IList<EnumCourseType>? Types { get; set; }

        public IList<EnumCourseLevel>? CourseLevels { get; set; }

        public string? PassportPath { get; set; }
        public string? UniversityDegreePath { get; set; }
        public string? CertificationPath { get; set; }
        public string? PoliceClearancePath { get; set; }

        [Required]
        public EnumRoleRegisterWithAdmin Role { get; set; }
    }
}
