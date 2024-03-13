// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Core.Entities;
    using Fsel.Identity.Domain.Enums;

    public class UserOtp : Entity
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Otp { get; set; }

        public DateTime ExpiredTime { get; set; }

        private EnumUserOtpStatus _status;

        public EnumUserOtpStatus Status
        {
            get { return (_status == EnumUserOtpStatus.New && ExpiredTime < DateTime.UtcNow) ? EnumUserOtpStatus.Expired : _status; }
            set { _status = value; }
        }

        public Guid? VerifyId { get; set; }

        public virtual User? User { get; set; }

        public virtual Guid? UserId { get; set; }
    }
}
