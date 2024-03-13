// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Quickstarts
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Attributes;

    public class ForgotModel
    {
        [EmailValid(ErrorMessage = "Email is not valid.")]
        [Required(ErrorMessage = "Email cannot be empty.")]
        public string? Email { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
