// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Constants
{
    public static class RegexSettings
    {
        public const string Password = "^(?=.*[A-Z])(?=.*\\d)(?=.*[\\W])[A-Za-z\\d\\W]{8,}$";
    }
}
