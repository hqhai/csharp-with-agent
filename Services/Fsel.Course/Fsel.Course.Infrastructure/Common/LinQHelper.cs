// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using System.Text.RegularExpressions;
    using Fsel.Common.Helpers;

    public class LinQHelper
    {
        public int GetHighestStreak(IList<bool>? data)
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

        public static bool IsValidSkillCode(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            // Regex: chỉ chấp nhận ký tự Latin (A-Z, a-z), số, ký tự đặc biệt ASCII, KHÔNG chứa khoảng trắng, KHÔNG unicode
            var regex = new Regex("^[\\x21-\\x7E]+$");

            return regex.IsMatch(input);
        }
    }
}
