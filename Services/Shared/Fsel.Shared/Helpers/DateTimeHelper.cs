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
            if (today > birthday?.AddYears(age))
            {
                age--;
            }
            return age;
        }
    }
}
