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
        [RegexValid(ErrorMessage = "Password is not valid.", Regex = RegexSettings.Password)]
        [Required(ErrorMessage = "Password cannot be empty.")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [RegexValid(ErrorMessage = "Confirm Password is not valid.", Regex = RegexSettings.Password)]
        [Display(Name = "Confirm Password")]
        [Compare(nameof(Password), ErrorMessage = "Password and confirmation password not match.")]
        [Required(ErrorMessage = "Password confirmation cannot be empty.")]
        public string? ConfirmPassword { get; set; }

        public Guid? VerifyId { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
