// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Common
{
    public static class LinQHelper
    {
        public static int GetHighestStreak(IList<bool>? data)
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
    }
}
