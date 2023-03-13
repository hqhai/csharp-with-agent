namespace Fsel.Identity.Domain.Models.CommandModels.Auths
{
    public class ConfirmEmailCommandModel
    {
        public string? Token { get; set; }
        public string? Email { get; set; }
    }
}
