// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Constants
{
    public static class RegexSettings
    {
        public const string PasswordValid = "^[A-Za-zÀ-ỹ'-]+(?: [A-Za-zÀ-ỹ'-]+)*$";
        public const string FullNameValid = @"^[A-Za-z\d!@#$%^&*(),.?"":{}|<>]{8,}$";
        public const string EmailValid = "^(?=.{1,64}@)(?=.{1,255}$)[a-zA-Z0-9!#$%&amp;'*+/=?^_{|}~-]+(?:\\.[a-zA-Z0-9!#$%&amp;'*+/=?^_{|}~-]+)*@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\\.)+[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?$\r\n ";
    }
}
