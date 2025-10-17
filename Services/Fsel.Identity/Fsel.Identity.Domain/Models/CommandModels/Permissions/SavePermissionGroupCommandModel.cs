namespace Fsel.Identity.Domain.Models.CommandModels.Permissions
{
    public class SavePermissionGroupCommandModel
    {
        public Guid? Id { get; set; }
        public string? ClaimType { get; set; }
        public string? Name { get; set; }
        public bool Status { get; set; }
        public string? Description { get; set; }
        //public Guid? MenuId { get; set; }
    }
}