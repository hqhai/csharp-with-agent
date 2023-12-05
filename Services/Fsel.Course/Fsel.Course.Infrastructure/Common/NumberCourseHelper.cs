// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using System.Collections;
    using Fsel.Common.Helpers;

    public class NumberCourseHelper
    {
        public double GetHighestStreak(object? data)
        {
            if (!(data is IList list))
            {
                return default;
            }
            var objects = list.Cast<object>().ToList();
            int longestStreak = 0;
            int currentStreak = 0;
            foreach (var item in objects)
            {
                var isFirstSubmit = item.GetPropValue<bool>("IsFirstSubmit");
                var isCorrect = item.GetPropValue<bool?>("IsCorrect");
                if (isFirstSubmit && isCorrect == true)
                {
                    currentStreak++;
                }
                else
                {
                    longestStreak = Math.Max(longestStreak, currentStreak);
                    currentStreak = 0;
                }
            }

            return Math.Max(longestStreak, currentStreak);
        }
    }
}
