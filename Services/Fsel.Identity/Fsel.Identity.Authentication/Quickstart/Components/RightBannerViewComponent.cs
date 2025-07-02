// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Authentication.Quickstart.Components
{
    using Microsoft.AspNetCore.Mvc;

    public class RightBannerViewComponent : ViewComponent
    {
        public RightBannerViewComponent()
        {
        }

        public async Task<IViewComponentResult> InvokeAsync(string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
    }
}
