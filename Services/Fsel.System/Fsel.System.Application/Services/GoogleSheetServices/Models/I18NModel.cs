// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.GoogleSheetServices.Models
{
    public class I18NModel : Dictionary<string, Dictionary<string, string>>
    {
        public void AddLanguage(string languageCode, string key, string value)
        {
            if (!ContainsKey(languageCode))
            {
                this[languageCode] = new Dictionary<string, string>();
            }

            if (!this[languageCode].ContainsKey(key))
            {
                this[languageCode].Add(key, value);
            }
        }
    }
}
