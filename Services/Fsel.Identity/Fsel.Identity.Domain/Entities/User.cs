using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Fsel.Identity.Domain.Entities
{
    public class User : IdentityUser
    {
        [Required]
        [MaxLength(250)]
        public string? FullName { get; set; }

        [MaxLength(1000)]
        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}