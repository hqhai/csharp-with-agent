using System.ComponentModel.DataAnnotations;

namespace Fsel.Identity.Common.Models.Commands
{
    public class RefreshTokenCommandModel
    {
        [Required]
        public string? AccessToken { get; set; }

        [Required]
        public string? RefreshToken { get; set; }
    }
}