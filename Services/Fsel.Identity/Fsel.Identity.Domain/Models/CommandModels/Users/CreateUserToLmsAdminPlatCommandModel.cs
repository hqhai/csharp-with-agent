// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Users
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Enums;

    public class CreateUserToLmsAdminPlatCommandModel
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? FullName { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MinLength(6, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public string? UserName { get; set; }

        public string? Email { get; set; }

        public string? Address { get; set; }

        public DateTime? Birthday { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? PhoneNumber { get; set; }

        public Guid? ManageUserId { get; set; }

        public string? Password { get; set; }

        public Guid? UserGroupId { get; set; }

        public EnumGender? Gender { get; set; }
    }
}
