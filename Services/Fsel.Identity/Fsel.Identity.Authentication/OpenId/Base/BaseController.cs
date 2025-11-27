// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Authentication.OpenId.Base
{
    using Microsoft.AspNetCore.Mvc;

    public class BaseController : Controller
    {
        protected object? GetFromTempData(string key)
        {
            var value = TempData[key];
            TempData[key] = value;
            return value;
        }
    }
}
