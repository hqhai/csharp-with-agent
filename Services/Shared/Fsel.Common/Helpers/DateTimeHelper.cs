// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Common.Helpers
{
    using System.Globalization;

    public static class DateTimeHelper
    {
        public static DateTime ConvertUnixTimeStampToDateTime(this long unixTimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        public static DateTime ConvertTimeFromUtc(this DateTime dateTime, TimeZoneInfo info)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(dateTime, info);
        }

        public static TimeSpan ConvertTimeSpan(this string? value)
        {
            if (!TimeSpan.TryParse(value, out TimeSpan result))
            {
                result = default;
            }
            return result;
        }

        public static int ConvertInt()
        {
            int weekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return weekNumber;
        }
    }
}
