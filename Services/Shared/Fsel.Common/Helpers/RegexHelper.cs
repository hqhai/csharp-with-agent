// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Common.Helpers
{
    using System.Text.RegularExpressions;

    public static class RegexHelper
    {
        public static bool IsValidEmail(this string? value)
        {
            if (value == null)
            {
                return false;
            }
            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Match match = regex.Match(value);
            return match.Success;
        }

        public static bool IsValidEmail(this IList<string>? values)
        {
            if (values == null)
            {
                return false;
            }
            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            foreach (var value in values)
            {
                if (!regex.IsMatch(value))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
