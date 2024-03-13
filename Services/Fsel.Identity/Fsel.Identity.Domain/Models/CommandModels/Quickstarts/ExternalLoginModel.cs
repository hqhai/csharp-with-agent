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
        public ClaimsPrincipal? Principal { get; set; }
    }
}
