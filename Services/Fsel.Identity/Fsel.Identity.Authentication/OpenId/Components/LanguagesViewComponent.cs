// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Authentication.OpenId.Components
{
    using Fsel.Common.Constants;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Microsoft.AspNetCore.Mvc;

    public class LanguagesViewComponent : ViewComponent
    {
        private readonly AuthContext _languageContext;

        public LanguagesViewComponent(AuthContext languageContext)
        {
            _languageContext = languageContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            ViewBag.LanguageContext = _languageContext;
            return View();
        }
    }
}
