using Fsel.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace Fsel.Identity.Domain.Models.CommandModels.Auths
{
    public class SignUpCommandModel
    {
        [Required]
        public string? FullName { get; set; }

        public string? PhoneNumber { get; set; }

        [Required, EmailAddress]
        public string? Email { get; set; }

        [Required]
        [Compare(nameof(ConfirmPassword))]
        public string? Password { get; set; }

        [Required]
        public string? ConfirmPassword { get; set; }

        [Required]
        public EnumRoleRegister Role { get; set; }
    }
}
