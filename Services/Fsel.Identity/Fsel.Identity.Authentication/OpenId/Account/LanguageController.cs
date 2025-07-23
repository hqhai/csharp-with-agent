// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Authentication.OpenId.Account
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Fsel.Identity.Authentication.OpenId.Base;
    using Fsel.Common.Helpers;
    using Fsel.Common.Constants;

    [AllowAnonymous]
    public class LanguageController : BaseController
    {
        private readonly Core.Base.AuthContext _languageContext;

        public LanguageController(Core.Base.AuthContext languageContext)
        {
            _languageContext = languageContext;
        }

        public IActionResult Change(string? culture, string? returnUrl)
        {
            _languageContext.AcceptLanguage = culture;
            HttpContext.SetCookie(Settings.RequestHeader.AcceptLanguage, culture);
            if (string.IsNullOrEmpty(returnUrl))
            {
                return RedirectToAction("Login", "Account");
            }
            return Redirect(returnUrl);
        }
    }
}
