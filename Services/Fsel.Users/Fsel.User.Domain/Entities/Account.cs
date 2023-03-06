using Fsel.Core.Entities;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Fsel.User.Domain.Entities
{
    public class Account : IdEntity
    {
        [Required]
        [MaxLength(250)]
        public string? FullName { get; set; }

        [MaxLength(1000)]
        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}
