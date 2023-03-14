using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

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

        public virtual Human? Human { get; set; }
    }
}
