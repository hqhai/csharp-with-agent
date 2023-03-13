using System.ComponentModel.DataAnnotations;

namespace Fsel.Identity.Domain.Models.CommandModels.Auths
{
    public class LoginCommandModel
    {
        [Required]
        public string? Username { get; set; }

        [Required]
        public string? Password { get; set; }
    }
}
