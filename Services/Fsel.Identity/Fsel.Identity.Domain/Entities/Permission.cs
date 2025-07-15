using Fsel.Common.Enums.ErrorCodes;
using System.ComponentModel.DataAnnotations;
using Fsel.Core.Entities;

namespace Fsel.Identity.Domain.Entities
{
    public class Permission : Entity
    {
        [Required]
        [MaxLength(100, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? ClaimValue { get; set; }

        [Required]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        public bool Status { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Description { get; set; }

        public Guid PermissionGroupId { get; set; }
        public PermissionGroup? PermissionGroup { get; set; }
        public ICollection<RoleClaim> RoleClaims { get; set; } = new List<RoleClaim>();
    }
}
