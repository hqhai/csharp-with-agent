// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Authentication.OpenId.Components
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Core.Entities.Tenants;
    using Microsoft.AspNetCore.Mvc;

    public class LogosViewComponent : ViewComponent
    {
        private readonly IServiceProvider _serviceProvider;

        public LogosViewComponent(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var tenantProvider = _serviceProvider.GetService<ITenantProvider>();
            Tenant? tenant = tenantProvider != null ? await tenantProvider.GetCurrentTenantAsync() : null;
            ViewBag.LogoPath = tenant?.LogoPath;
            return View();
        }
    }
}
