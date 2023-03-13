using System.ComponentModel.DataAnnotations;

namespace Fsel.Identity.Domain.Models.CommandModels.Auths
{
    public class RefreshTokenCommandModel
    {
        [Required]
        public string? AccessToken { get; set; }

        [Required]
        public string? RefreshToken { get; set; }
    }
}
