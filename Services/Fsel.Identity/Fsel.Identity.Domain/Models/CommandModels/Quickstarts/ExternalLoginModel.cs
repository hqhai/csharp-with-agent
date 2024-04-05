// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Quickstarts
{
    using System.ComponentModel.DataAnnotations;
    using System.Security.Claims;
    using Fsel.Common.Attributes;

    public class ExternalLoginModel
    {
        [EmailValid(ErrorMessage = "Email is not valid.")]
        [Required(ErrorMessage = "Email cannot be empty.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "First name cannot be empty.")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Last name cannot be empty.")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Birth day cannot be empty.")]
        public DateTime? Birthday { get; set; }

        public string? Provider { get; set; }

        public ClaimsPrincipal? Principal { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
