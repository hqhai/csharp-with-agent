// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;

using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;

namespace Fsel.Master.Identity.Domain.Entities
{
    public class MasterUserToken : UserTokenEntity
    {
        [MaxLength(50, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public override string? RefreshToken { get; set; }
    }
}
