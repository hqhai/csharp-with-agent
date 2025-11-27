// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Authentication.OpenId.Components
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
