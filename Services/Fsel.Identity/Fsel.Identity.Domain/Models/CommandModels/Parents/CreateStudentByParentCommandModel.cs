// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Parents
{
    using System.ComponentModel.DataAnnotations;

    public class CreateStudentByParentCommandModel
    {
        [Required]
        public string? FullName { get; set; }

        [Required]
        public string? UserName { get; set; }

        [Required]
        public string? Password { get; set; }
        public string? School { get; set; }
        [Required]
        public string? AvatarPath { get; set; }
    }
}
