// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;

namespace Fsel.Identity.Domain.Entities
{
    public class UserGroup : Entity
    {
        [Required]
        [MaxLength(100, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? GroupName { get; set; }

        public int DisplayOrder { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public string? LocationIdStr { get; set; }

        public virtual ICollection<UserGroupMemberShip> Members { get; set; } = new List<UserGroupMemberShip>();
    }
}
