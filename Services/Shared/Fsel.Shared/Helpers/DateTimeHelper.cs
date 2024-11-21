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

        public static string ConvertSecondsToTimeString(long totalSeconds)
        {
            int hours = (int)totalSeconds / 3600;
            int minutes = (int)(totalSeconds % 3600) / 60;
            return string.Format(CultureInfo.InvariantCulture, "{0}h{1:D2}'", hours, minutes);
        }

        public static int ConvertSecondsToHours(long seconds)
        {
            return (int)seconds / 3600;
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

        public static (DateTime Monday, DateTime Sunday) GetMondayAndSunday(DateTime date)
        {
            // Tìm ngày Thứ Hai
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            DateTime monday = date.AddDays(-1 * diff).Date;

            // Tìm ngày Chủ Nhật
            DateTime sunday = monday.AddDays(6).Date;

            return (monday, sunday);
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

        public static long ConvertDateTimeToSeconds(this DateTime dateTime)
        {
            long totalSeconds = dateTime.Hour * 3600 + dateTime.Minute * 60;
            return totalSeconds;
        }
    }
}
