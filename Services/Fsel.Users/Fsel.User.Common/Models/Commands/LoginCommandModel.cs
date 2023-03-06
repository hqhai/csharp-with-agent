using System.ComponentModel.DataAnnotations;

namespace Fsel.User.Common.Models.Commands
{
    public class LoginCommandModel
    {
        [Required]
        public string? Username { get; set; }

        [Required]
        public string? Password { get; set; }
    }
}