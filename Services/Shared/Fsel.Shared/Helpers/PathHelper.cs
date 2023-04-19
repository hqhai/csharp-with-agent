// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    public static class PathHelper
    {
        public static string? Combine(string? path1, string? path2)
        {
            if (path1 != null && path2 != null)
            {
                return $"{path1}/{path2}";
            }
            return path1 ?? path2;

        }

        public static string? AddScheme(string? url, string? scheme = null)
        {
            if (scheme == null)
            {
                scheme = Uri.UriSchemeHttps;
            }
            return $"{scheme}://{url}";
        }
    }
}
