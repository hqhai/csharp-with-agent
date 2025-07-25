namespace Fsel.Identity.Domain.Models.CommandModels.Permissions
{
    public class SaveRoleClaimCommandModels
    {
        public Guid RoleId { get; set; }
        public IList<SaveRoleClaimCommandModel>? RoleClaims { get; set; }
    }

    public class SaveRoleClaimCommandModel
    {
        public Guid PermissionGroupId { get; set; }
        public Guid PermissionId { get; set; }
    }
}
