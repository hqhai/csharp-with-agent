// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Quickstarts
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Attributes;
    using Fsel.Identity.Domain.Constants;

    public class ForgotPasswordModel
    {
        public string? Email { get; set; }

        [DataType(DataType.Password)]
        [RegexValid(ErrorMessage = "i18n_Password_is_not_valid", Regex = RegexSettings.Password)]
        [Required(ErrorMessage = "i18n_Password_cannot_be_empty")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "i18n_confirmation_password")]
        [Compare(nameof(Password), ErrorMessage = "i18n_Password_and_Confirm_Password_not_match")]
        [RegexValid(ErrorMessage = "i18n_Confirm_Password_is_not_valid.", Regex = RegexSettings.Password)]
        [Required(ErrorMessage = "i18n_Confirm_Password_cannot_be_empty.")]
        public string? ConfirmPassword { get; set; }

        public Guid? VerifyId { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
