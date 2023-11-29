// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using System;
    using System.Globalization;
    using Fsel.Core.Entities;

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

        public static double GetWorkingTime(DateTime inputDate, DateTime outputDate, double executionTime)
        {
            if (executionTime != default)
            {
                var sectionBetweenDate = GetSecondBetweenDate(inputDate, outputDate);
                return sectionBetweenDate <= executionTime ? sectionBetweenDate : executionTime;
            }
            return default;
        }

        public static double GetSecondBetweenDate(DateTime inputDate, DateTime outputDate)
        {
            return NumberHelper.ConvertRound((outputDate - inputDate).TotalSeconds);
        }

        public static DateTime GetDateTimeEntity(Entity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            return entity.UpdatedDate ?? entity.CreatedDate;
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
                    var numberOfDays = DateTime.UtcNow.Date - date;
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
    }
}
