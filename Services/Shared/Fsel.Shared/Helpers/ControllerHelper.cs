// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Web;
    using Microsoft.AspNetCore.Mvc;

    public static class ControllerHelper
    {
        public static RedirectResult RedirectWithQuery(this Controller controller, string url, object queryParams)
        {
            if (queryParams == null)
            {
                return controller.Redirect(url);
            }

            var queryDict = new Dictionary<string, string>();
            foreach (var prop in queryParams.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = prop.GetValue(queryParams);
                if (value != null)
                {
                    queryDict[prop.Name] = value.ToString();
                }
            }

            // Build query string
            var query = string.Join("&", queryDict.Select(kvp =>
                $"{HttpUtility.UrlEncode(kvp.Key)}={HttpUtility.UrlEncode(kvp.Value)}"));

            var separator = url.Contains("?") ? "&" : "?";
            var finalUrl = $"{url}{separator}{query}";

            return controller.Redirect(finalUrl);
        }
    }
}
