// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Students
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;

    public class UpdateProfileStudentCommandModel
    {
        public string? AvatarPath { get; set; }

        [Required]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? FullName { get; set; }

        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
