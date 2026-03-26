// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using System;
    using System.Globalization;
    using Fsel.Common.Helpers;

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

            if (today < birthday?.AddYears(age))
            {
                age--;
            }

            #endregion Bỏ check tuổi theo ngày chỉ tính năm sinh

            return age;
        }

        public static double GetSecondBetweenDate(DateTime inputDate, DateTime outputDate)
        {
            return NumberHelper.ConvertRound((outputDate - inputDate).TotalSeconds);
        }

        public static async Task<(int, bool)> CountContinuousDaysAsync(IList<DateTime>? dates)
        {
            if (dates == null || dates.Count == 0)
            {
                return (0, false);
            }
            else if (dates.Count == 1)
            {
                var difference = DateTime.UtcNow.Date - dates[0].Date;
                return (difference.Days <= 1 ? 1 : 0, difference.Days <= 1);
            }
            else
            {
                var sortedDates = dates.OrderByDescending(date => date.Date).ToList();
                int consecutiveDays = 0;

                foreach (var date in sortedDates)
                {
                    var numberOfDays = DateTime.UtcNow.Date - date.Date;
                    if (numberOfDays.Days == consecutiveDays + 1)
                    {
                        consecutiveDays++;
                    }
                    else
                    {
                        continue;
                    }
                    await Task.Delay(0);
                }
                if (DateTime.UtcNow.Date == sortedDates[0].Date)
                {
                    consecutiveDays++;
                }
                var firstDateDifference = DateTime.UtcNow.Date - sortedDates[0].Date;
                return (firstDateDifference.Days <= 1 ? consecutiveDays : 0, firstDateDifference.Days <= 1);
            }
        }

        public static ICollection<DateTime> GenerateDateList(DateTime startDate, DateTime endDate)
        {
            List<DateTime> dateList = new List<DateTime>();

            while (startDate <= endDate)
            {
                dateList.Add(startDate);
                startDate = startDate.AddDays(1);
            }

            return dateList;
        }

        public static int ConvertSecondsToMinutes(long seconds)
        {
            long minutes = seconds / 60;
            return (int)minutes;
        }

        public static ICollection<DateTime> GetWeekDays(DateTime date)
        {
            // Tìm ngày thứ Hai của tuần chứa ngày ngẫu nhiên
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            DateTime monday = date.AddDays(-1 * diff).Date;

            // Tạo danh sách các ngày từ thứ Hai đến Chủ nhật
            List<DateTime> weekDays = new List<DateTime>();
            for (int i = 0; i < 7; i++)
            {
                weekDays.Add(monday.AddDays(i));
            }

            return weekDays;
        }

        public static (DateTime Monday, DateTime Sunday) GetMondayAndSunday(DateTime date)
        {
            // Tìm ngày Thứ Hai
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            DateTime monday = date.AddDays(-1 * diff).Date;

            // Tìm ngày Chủ Nhật
            DateTime sunday = monday.AddDays(6).Date;

            return (monday, sunday);
        }

        public static string ConvertSecondsToTimeString(long totalSeconds)
        {
            int hours = (int)totalSeconds / 3600;
            int minutes = (int)(totalSeconds % 3600) / 60;
            return string.Format(CultureInfo.InvariantCulture, "{0}h{1:D2}'", hours, minutes);
        }

        public static object ConvertSecondsToHours(long seconds)
        {
            double hours = (double)seconds / 3600;
            return hours >= 1 ? (object)(int)hours : Math.Round(hours, 1);
        }

        public static int ConvertSecondsToHoursRoundUp(long seconds)
        {
            int hours = (int)seconds / 3600;
            if (seconds % 3600 > 0)
            {
                hours += 1;
            }
            return hours;
        }

        public static string ConvertSecondsToHoursAndMinutes(long seconds)
        {
            int hours = (int)seconds / 3600;
            int minutes = (int)(seconds % 3600) / 60;

            return $"{hours} giờ {minutes:D2} phút";
        }

        public static bool IsCurrentDateInRange(DateTime? startDate, DateTime? endDate)
        {
            var currentDate = DateTime.UtcNow;

            if (endDate.HasValue && startDate.HasValue && startDate.Value.Date <= currentDate && endDate.Value.Date >= currentDate.Date)
            {
                return true;
            }
            else if (!endDate.HasValue && startDate.HasValue && startDate.Value.Date <= currentDate)
            {
                return true;
            }

            return false;
        }

        public static DateTime? GetDayByDayOfWeek(this DateTime currentDay, IList<DayOfWeek>? dayOfWeeks)
        {
            ArgumentNullException.ThrowIfNull(dayOfWeeks);

            for (int i = 1; i <= 7; i++)
            {
                DateTime candidateDate = currentDay.AddDays(i);
                if (dayOfWeeks.Contains(candidateDate.DayOfWeek))
                {
                    return candidateDate.Date;
                }
            }

            return null;
        }

        public static (int, int) ConvertHoursAndMinutesBySeconds(this long seconds)
        {
            int hours = (int)seconds / 3600;
            int minutes = (int)(seconds % 3600) / 60;

            return (hours, minutes);
        }

        public static long ConvertDateTimeToSeconds(this DateTime dateTime)
        {
            long totalSeconds = dateTime.Hour * 3600 + dateTime.Minute * 60 + dateTime.Second;
            return totalSeconds;
        }

        public static bool IsValidDateTime(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            DateTime parsedDate;
            var culture = CultureInfo.InvariantCulture;

            string[] formats = { "yyyy-MM-dd", "dd-MM-yyyy", "yyyy/MM/dd", "dd/MM/yyyy" };

            return DateTime.TryParseExact(input, formats, culture, DateTimeStyles.None, out parsedDate);
        }

        public static DateTime ConvertToDateTime(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return DateTime.UtcNow;

            if (IsValidDateTime(input))
            {
                var culture = CultureInfo.InvariantCulture;
                string[] formats = { "yyyy-MM-dd", "dd-MM-yyyy", "yyyy/MM/dd", "dd/MM/yyyy" };

                if (DateTime.TryParseExact(input, formats, culture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    return parsedDate;
                }
            }
            return DateTime.UtcNow;
        }

        public static int CalculateAge(DateTime birthday, DateTime currentDate)
        {
            int age = currentDate.Year - birthday.Year;
            if (currentDate < birthday.AddYears(age))
                age--;
            return age;
        }

        public static (DateTime weekStartUtc, DateTime weekEndUtc) GetCurrentWeekRangeNow(DateTime? dateTime = default)
        {
            var nowUtc = dateTime ?? DateTime.UtcNow;
            var nowVn = nowUtc.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date;

            var startVn = GetWeekStartMonday(nowVn);
            var endVn = startVn.AddDays(6);
            return (startVn, endVn);
        }

        public static (DateTime weekStartUtc, DateTime weekEndUtc) GetCurrentWeekRangeUtc(DateTime? dateTime = default)
        {
            var nowUtc = dateTime ?? DateTime.UtcNow.Date;
            var nowVn = nowUtc.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date;

            var startVn = GetWeekStartMonday(nowVn);
            var endVn = startVn.AddDays(6);
            return (startVn.ConvertTimeToUtc(EnumCountryKey.UTC), endVn.ConvertTimeToUtc(EnumCountryKey.UTC));
        }

        public static DateTime GetWeekStartMonday(DateTime date)
        {
            var day = (int)date.DayOfWeek; // Sunday=0 ... Monday=1 ... Saturday=6
            var offset = day == 0 ? -6 : 1 - day; // về thứ 2
            return date.AddDays(offset);
        }

        public static (DateTime MondayStart, DateTime SundayEnd) GetWeekBoundaries(DateTime anyDate)
        {
            int dayOfWeek = (int)anyDate.DayOfWeek;

            // Tính ngày thứ Hai
            DateTime monday = anyDate.AddDays(dayOfWeek == 0 ? -6 : 1 - dayOfWeek)
                                     .Date; // 00:00:00

            // Tính ngày Chủ Nhật
            DateTime sunday = monday.AddDays(6).Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            return (monday, sunday);
        }
    }
}
