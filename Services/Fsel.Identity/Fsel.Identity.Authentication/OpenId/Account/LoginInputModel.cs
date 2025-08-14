// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace Fsel.Identity.Authentication.OpenId.Account
{
    public class LoginInputModel
    {
        [Required(ErrorMessage = "i18n_Username_cannot_be_empty")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "i18n_Password_cannot_be_empty")]
        public string? Password { get; set; }

        public bool RememberLogin { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
