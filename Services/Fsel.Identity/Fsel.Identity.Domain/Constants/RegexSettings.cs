// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Constants
{
    public static class RegexSettings
    {
        public const string PasswordValid = "^(?=.*[A-Z])(?=.*[a-z])(?=.*\\d)(?=.*[!@#$%^&*(),.?\":{}|<>]).{8,}$";
        public const string FullNameValid = @"^[A-Za-zÀ-ỹ\s'-]+$";
        public const string EmailValid = @"^[a-z0-9!#$%&'*+/=?^_`{|}~-]{1,64}(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@[a-z0-9](?:[a-z0-9-]{0,253}[a-z0-9])?(?:\.[a-z0-9](?:[a-z0-9-]{0,253}[a-z0-9])?)*$";
    }
}
