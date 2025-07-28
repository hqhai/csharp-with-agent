namespace Fsel.Identity.Domain.Models.CommandModels.Permissions
{
    public class SavePermissionCommandModel
    {
        public Guid? Id { get; set; }
        public string? ClaimValue { get; set; }
        public string? Name { get; set; }
        public bool Status { get; set; }
        public string? Description { get; set; }
        public Guid PermissionGroupId { get; set; }
    }
}
