// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using System;
    using System.Globalization;

    public static class DateTimeHelper
    {
        public static string? ToStringTime(this DateTime? dateTime, string format = "HH:mm")
        {
            return dateTime.HasValue ? dateTime.Value.ToString(format, CultureInfo.InvariantCulture) : null;
        }

        public static int GetYearOld(DateTime? birthday)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - birthday?.Year ?? default;
            #region Bỏ check tuổi theo ngày chỉ tính năm sinh
            //if (today > birthday?.AddYears(age))
            //{
            //    age--;
            //}
            #endregion
            return age;
        }

        public static async Task<(int, bool)> CountContinuousDaysAsync(IList<DateTime>? dates)
        {
            if (dates != null && dates.Count > 0)
            {
                int count = 0;
                DateTime? previousDate = null;
                bool isDaysStreakIncrease = true;
                foreach (var date in dates)
                {
                    if (previousDate == null || (date - previousDate.Value).TotalDays == 1)
                    {
                        count++;
                        isDaysStreakIncrease = true;
                    }
                    else
                    {
                        count = 1;
                        isDaysStreakIncrease = false;
                    }
                    previousDate = date;
                    await Task.Delay(1);
                }
                return (count, isDaysStreakIncrease);
            }
            return (0, false);
        }
    }
}
