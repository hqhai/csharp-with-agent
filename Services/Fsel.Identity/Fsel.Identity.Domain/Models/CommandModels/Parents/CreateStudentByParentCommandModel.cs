// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Parents
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Identity.Domain.Enums;

    public class CreateStudentByParentCommandModel
    {
        [Required]
        public string? Name { get; set; }

        [Required]
        public string? UserName { get; set; }

        public DateTime? BirthDay { get; set; }

        public EnumGender Gender { get; set; }

        [Required]
        [Compare(nameof(ConfirmPassword))]
        public string? Password { get; set; }

        [Required]
        public string? ConfirmPassword { get; set; }

        [Required]
        public string? School { get; set; }

        [Required]
        public string? AvatarPath { get; set; }
    }
}
