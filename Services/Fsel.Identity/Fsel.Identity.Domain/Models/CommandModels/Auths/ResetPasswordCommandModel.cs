using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Fsel.Identity.Domain.Models.CommandModels.Auths
{
    public class ResetPasswordCommandModel
    {
        [Required]
        public string? Email { get; set; }

        [Required]
        public string? OldPassword { get; set; }

        [Required]
        [Compare(nameof(ConfirmPassword))]
        public string? Password { get; set; }

        [Required]
        public string? ConfirmPassword { get; set; }
    }
}
