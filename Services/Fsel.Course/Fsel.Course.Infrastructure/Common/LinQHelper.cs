// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using System.Text.RegularExpressions;

    public static class LinQHelper
    {
        public static int GetHighestStreak(this IList<bool>? data)
        {
            if (data != null && data.Any())
            {
                var highestStreak = data.Aggregate(new { CurrentStreak = 0, MaxStreak = 0 },
                                (acc, value) => new
                                {
                                    CurrentStreak = value ? acc.CurrentStreak + 1 : 0,
                                    MaxStreak = value ? Math.Max(acc.MaxStreak, acc.CurrentStreak + 1) : acc.MaxStreak
                                })
                            .MaxStreak - 1;
                return highestStreak > 0 ? highestStreak : default;
            }
            return default;
        }

        public static bool IsValidCode(this string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }
            var regex = new Regex(@"^[^<>]*$");
            return regex.IsMatch(input);
        }
    }
}
