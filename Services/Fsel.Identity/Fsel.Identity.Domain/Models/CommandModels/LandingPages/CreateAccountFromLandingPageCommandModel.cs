// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.LandingPages
{
    using System;

    public class CreateAccountFromLandingPageCommandModel
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public DateTime Birthday { get; set; }
        public string? Password { get; set; }
    }
}
