using System.ComponentModel.DataAnnotations;

namespace Fsel.Identity.Common.Models.Commands
{
    public class LoginCommandModel
    {
        [Required]
        public string? Username { get; set; }

        [Required]
        public string? Password { get; set; }
    }
}