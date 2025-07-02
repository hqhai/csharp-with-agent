// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Enums.ErrorCodes;
using System.ComponentModel.DataAnnotations;
using Fsel.Core.Entities;

namespace Fsel.Identity.Domain.Entities
{
    public class UserToken : UserTokenEntity
    {
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? IpAddress { get; set; }

        [MaxLength(50, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public override string? RefreshToken { get; set; }
    }
}
