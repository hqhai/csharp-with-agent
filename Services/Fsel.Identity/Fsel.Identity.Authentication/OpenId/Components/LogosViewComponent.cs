// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Authentication.OpenId.Components
{
    using Microsoft.AspNetCore.Mvc;

    public class LogosViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View();
        }
    }
}
