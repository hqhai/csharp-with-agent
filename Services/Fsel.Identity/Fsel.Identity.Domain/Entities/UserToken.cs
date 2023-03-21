using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums.ErrorCodes;
using Microsoft.AspNetCore.Identity;

namespace Fsel.Identity.Domain.Entities
{
    public class UserToken : IdentityUserToken<string>
    {
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}
