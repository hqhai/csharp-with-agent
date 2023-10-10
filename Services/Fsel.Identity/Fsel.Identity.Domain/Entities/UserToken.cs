// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;

namespace Fsel.Identity.Domain.Entities
{
    public class UserToken : UserTokenEntity
    {
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}
