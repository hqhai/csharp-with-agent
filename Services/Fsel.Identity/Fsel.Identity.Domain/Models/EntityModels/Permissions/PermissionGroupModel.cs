namespace Fsel.Identity.Domain.Models.EntityModels.Permissions
{
    using Fsel.Core.Base.BaseModels;

    public class PermissionGroupModel : BaseModel
    {
        public string? ClaimType { get; set; }
        public string? Name { get; set; }
        public bool Status { get; set; }
        public string? Description { get; set; }
        public Guid? MenuId { get; set; }
        public IList<PermissionModel>? Permissions { get; set; }
    }
}
