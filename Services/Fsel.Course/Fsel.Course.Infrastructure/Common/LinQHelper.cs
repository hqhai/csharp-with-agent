// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    public class LinQHelper
    {
        public int GetHighestStreak(IList<bool>? data)
        {
            if (data != null && data.Any())
            {
                return data.Aggregate(new { CurrentStreak = 0, MaxStreak = 0 },
                                 (acc, value) => new
                                 {
                                     CurrentStreak = value ? acc.CurrentStreak + 1 : 0,
                                     MaxStreak = value ? Math.Max(acc.MaxStreak, acc.CurrentStreak + 1) : acc.MaxStreak
                                 })
                             .MaxStreak - 1;
            }
            return default;
        }
    }
}
