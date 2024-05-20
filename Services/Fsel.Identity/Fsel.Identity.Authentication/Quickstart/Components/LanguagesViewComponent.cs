// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Authentication.Quickstart.Components
{
    using Fsel.Core.Base;
    using Microsoft.AspNetCore.Mvc;

    public class LanguagesViewComponent : ViewComponent
    {
        private readonly LanguageContext _languageContext;

        public LanguagesViewComponent(LanguageContext languageContext)
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
