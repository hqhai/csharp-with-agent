// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;

using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;

namespace Fsel.Master.Identity.Domain.Entities
{
    public class MasterRole : RoleEntity
    {
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Description { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }

        public bool IsDefault { get; set; }
    }
}
