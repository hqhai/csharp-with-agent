// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.OpenId
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Identity.Domain.Constants;

    public class ResetPasswordModel : IRequestBodyTenantAware
    {
        [Required]
        public string? Identity { get; set; }

        [DataType(DataType.Password)]
        [RegularExpression(RegexSettings.PasswordValid, ErrorMessage = "i18n_Password_is_not_valid")]
        [Required(ErrorMessage = "i18n_Password_cannot_be_empty")]
        public string? Password { get; set; }

        [Required]
        public string Token { get; set; }

        public string? ReturnUrl { get; set; }

        public string? UserName { get; set; }

        public Guid? UserId { get; set; }
    }
}
