// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.OpenId
{
    using System.ComponentModel.DataAnnotations;

    public class ForgotModel
    {
        [Required(ErrorMessage = "i18nFieldRequired")]
        public string? Identity { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
