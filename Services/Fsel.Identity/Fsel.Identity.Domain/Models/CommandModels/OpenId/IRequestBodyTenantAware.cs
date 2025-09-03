namespace Fsel.Identity.Domain.Models.CommandModels.OpenId
{
    public interface IRequestBodyTenantAware
    {
        string? Identity { get; set; }
        string? UserName { get; set; }
        Guid? UserId { get; set; }
    }
}
