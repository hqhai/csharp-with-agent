// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    public class LinQHelper
    {
        public int GetHighestStreak(IList<bool>? data)
        {
            if (data != null && data.Any())
            {
                return data.Aggregate(
                                  new { Longest = 0, Current = 0 },
                                  (agg, element) => element ?
                                      new { Longest = agg.Current + 1 > agg.Longest ? agg.Current + 1 : agg.Longest, Current = agg.Current + 1 } :
                                      new { agg.Longest, Current = 0 },
                                  agg => agg.Longest);
            }
            return default;
        }
    }
}
