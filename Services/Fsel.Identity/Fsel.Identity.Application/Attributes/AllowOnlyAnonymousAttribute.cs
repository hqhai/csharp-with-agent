// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Attributes
{
    using System;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class AllowOnlyAnonymousAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _defaultUrl;

        public AllowOnlyAnonymousAttribute(string defaultUrl = "~/")
        {
            _defaultUrl = defaultUrl;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (user?.Identity != null && user.Identity.IsAuthenticated)
            {
                var query = context.HttpContext.Request.Query;

                var returnUrl = query.ContainsKey("returnUrl")
                    ? query["returnUrl"].ToString()
                    : null;
                if (string.IsNullOrWhiteSpace(returnUrl))
                {
                    returnUrl = _defaultUrl;
                }

                context.Result = new RedirectResult(returnUrl);
            }
        }
    }
}
