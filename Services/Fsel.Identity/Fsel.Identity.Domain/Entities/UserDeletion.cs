// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Attributes;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Identity.Domain.Enums;

    public class UserDeletion : Entity
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? FullName { get; set; }

        [PhoneValid(ErrorMessage = nameof(EnumSystemErrorCode.InValidFormat))]
        [MaxLength(20, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? PhoneNumber { get; set; }

        [EmailValid(ErrorMessage = nameof(EnumSystemErrorCode.InValidFormat))]
        [MaxLength(70, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Email { get; set; }

        public EnumUserDeletionReason Reason { get; set; }

        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? ReasonContent { get; set; }

        public DateTime DeletionDate { get; set; }
        public EnumUserDeletionStatus Status { get; set; }
        public virtual User? User { get; set; }
        public Guid? UserId { get; set; }
    }
}
