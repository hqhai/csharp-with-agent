namespace Fsel.Identity.Domain.Models.EntityModels.Permissions
{
    public class RoleClaimModel
    {
        public string? PermissionGroupName { get; set; }
        public string? PermissionName { get; set; }
        public string? ClaimType { get; set; }
        public string? ClaimValue { get; set; }
        public Guid PermissionGroupId { get; set; }
        public Guid PermissionId { get; set; }
        public bool PermissionGroupStatus { get; set; }
        public bool PermissionStatus { get; set; }
    }
}
