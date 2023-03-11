namespace Fsel.Identity.Common.Models.Commands
{
    public class ConfirmEmailCommandModel
    {
        public string? Token { get; set; }
        public string? Email { get; set; }
    }
}