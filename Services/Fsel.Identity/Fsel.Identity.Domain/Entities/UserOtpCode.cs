// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Core.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Shared.Enums;

    public class UserOtpCode : Entity
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(20, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? OtpCode { get; set; }

        public DateTime ExpiredTime { get; set; }

        public int RetryCount { get; set; }

        private EnumOtpCodeStatus _status;

        public EnumOtpCodeStatus Status
        {
            get { return (_status == EnumOtpCodeStatus.New && ExpiredTime < DateTime.UtcNow) ? EnumOtpCodeStatus.Expired : _status; }
            set { _status = value; }
        }

        public EnumUserOtpCodeType Type { get; set; }

        public Guid? VerifyId { get; set; }

        public virtual User? User { get; set; }

        public virtual Guid? UserId { get; set; }
    }
}
