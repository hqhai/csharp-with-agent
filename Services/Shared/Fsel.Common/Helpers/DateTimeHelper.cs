using Newtonsoft.Json.Linq;

namespace Fsel.Common.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTime ConvertUnixTimeStampToDateTime(this long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        public static DateTime ConvertTimeFromUtc(this DateTime dateTime, TimeZoneInfo info)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(dateTime, info);
        }
    }
}