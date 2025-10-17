using Fsel.Core.Entities;

namespace Fsel.Identity.Domain.Entities
{
    public class RoleClaim : RoleClaimEntity
    {
        public Guid PermissionGroupId { get; set; }
        public Guid PermissionId { get; set; }
        public bool Status { get; set; }
        public Permission? Permission { get; set; }
        public PermissionGroup? PermissionGroup { get; set; }
    }
}
