// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Constants
{
    public static class RegexSetting
    {
        public const string AiReponseJsonPattern = "^```json\\s*|\\s*```$";
        public const string Base64Pattern = @"^data:image\/(jpeg|jpg|png|gif|bmp|tiff);base64,([A-Za-z0-9+/]+={0,2})$";
        public const string SpecialCharacterPattern = @"[^a-zA-Z0-9]";
        public const string WordPattern = "[‘'’ʼ]";
        public const string EmailValid = @"^(?=.{1,64}@)(?=.{1,255}$)[a-zA-Z0-9!#$%&'*+/=?^_{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_{|}~-]+)*@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?$";
    }
}
