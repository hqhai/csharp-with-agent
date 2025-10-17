namespace Fsel.Identity.Domain.Models.EntityModels.Permissions
{
    using Fsel.Core.Base.BaseModels;

    public class PermissionModel : BaseModel
    {
        public string? ClaimValue { get; set; }
        public string? Name { get; set; }
        public bool Status { get; set; }
        public string? Description { get; set; }
        public string? PermissionGroupName { get; set; }
        public Guid PermissionGroupId { get; set; }
    }
}
