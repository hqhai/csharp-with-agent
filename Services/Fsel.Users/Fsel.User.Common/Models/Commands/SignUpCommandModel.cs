using Fsel.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace Fsel.User.Common.Models.Commands
{
    public class SignUpCommandModel
    {
        [Required]
        public string? FullName { get; set; }

        [Required, EmailAddress]
        public string? Email { get; set; }

        [Required]
        [Compare(nameof(ConfirmPassword))]
        public string? Password { get; set; }

        [Required]
        public string? ConfirmPassword { get; set; }

        [Required]
        public EnumRole Role { get; set; }
    }
}