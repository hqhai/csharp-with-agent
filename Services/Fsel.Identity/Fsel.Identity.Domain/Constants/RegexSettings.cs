// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Constants
{
    public static class RegexSettings
    {
        public const string PasswordValid = "^(?=.*[A-Z])(?=.*[a-z])(?=.*\\d)(?=.*[!@#$%^&*(),.?\":{}|<>]).{8,}$";
        public const string FullNameValid = @"^[A-Za-z\d!@#$%^&*(),.?"":{}|<>]{1,}$";
        public const string PhoneNumberValid = "^(?:\\+|(?=\\d{10}))\\d{10,15}$";
        public const string EmailValid = "^(?=.{1,64}@)(?=.{1,255}$)[a-zA-Z0-9!#$%&amp;'*+/=?^_{|}~-]+(?:\\.[a-zA-Z0-9!#$%&amp;'*+/=?^_{|}~-]+)*@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\\.)+[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?$\r\n ";
    }
}
