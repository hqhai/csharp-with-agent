// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Users
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using System.ComponentModel.DataAnnotations;

    public class UpdateUserCommandModel
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
    }
}
