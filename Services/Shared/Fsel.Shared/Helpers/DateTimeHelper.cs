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

            #endregion Bỏ check tuổi theo ngày chỉ tính năm sinh

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

        public static DateTime GetLastDayOfTheMonth(DateTime dtInput)
        {
            DateTime dtResult = dtInput;
            dtResult = dtResult.AddMonths(1);
            dtResult = dtResult.AddDays(-(dtResult.Day)).Date;
            return dtResult;
        }

        public static int GetDayInMonth(DateTime dtInput)
        {
            DateTime dtResult = dtInput;
            dtResult = dtResult.AddMonths(1);
            dtResult = dtResult.AddDays(-(dtResult.Day));
            return dtResult.Day;
        }

        public static DateTime GetFistDayOfTheMonth(DateTime dtInput)
        {
            DateTime dtResult = dtInput;
            dtResult = dtResult.AddDays((-dtResult.Day) + 1).Date;
            return dtResult;
        }
    }
}
