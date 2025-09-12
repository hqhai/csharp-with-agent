// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Authentication.OpenId.Components
{
    using Fsel.Core.Base.Interfaces;
    using Microsoft.AspNetCore.Mvc;

    public class LogosViewComponent : ViewComponent
    {
        private readonly ITenantProvider _tenantProvider;

        public LogosViewComponent(ITenantProvider tenantProvider)
        {
            _tenantProvider = tenantProvider;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var tenant = await _tenantProvider.GetCurrentTenantAsync();
            ViewBag.LogoPath = tenant?.LogoPath;
            return View();
        }
    }
}
