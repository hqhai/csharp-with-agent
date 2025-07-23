// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.OpenId
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Identity.Domain.Constants;

    public class ForgotModel
    {
        [RegularExpression(RegexSettings.EmailValid, ErrorMessage = "i18n_Email_is_not_valid")]
        [Required(ErrorMessage = "i18n_Email_cannot_be_empty")]
        public string? Email { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
