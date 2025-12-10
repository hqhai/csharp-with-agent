// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using System.Text;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using OtpNet;
    using static Fsel.Shared.Constants.ValueSettings;

    public static class NumberHelper
    {
        public static double RoundReduceNumber(double number)
        {
            return Math.Floor(number * 2) / 2;
        }

        public static double RoundToQuarter(double number)
        {
            double multiplied = number * 4;
            double rounded = Math.Round(multiplied);
            return rounded / 4;
        }

        public static double RoundNumberDouble(double number)
        {
            double fractionalPart = number - Math.Floor(number);
            if (fractionalPart < 0.25)
            {
                return Math.Floor(number);
            }
            else if (fractionalPart < 0.75)
            {
                return Math.Floor(number) + 0.5;
            }
            else
            {
                return Math.Ceiling(number);
            }
        }

        public static string GetRandomCode()
        {
            var randomSecure = new RandomSecureHelper();
            var totp = new Totp(Encoding.UTF8.GetBytes(randomSecure.Secretstrings()));
            return totp.ComputeTotp();
        }

        public static string GenerateCode(int length)
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            Random random = new Random();

            string orderCode = new string(Enumerable.Repeat(chars, length)
                                      .Select(s => s[random.Next(s.Length)]).ToArray());

            return orderCode;
        }

        public static string GenerateCodeNumber(int length)
        {
            string chars = "0123456789";

            Random random = new Random();

            string orderCode = new string(Enumerable.Repeat(chars, length)
                                      .Select(s => s[random.Next(s.Length)]).ToArray());

            return orderCode;
        }

        public static double ConvertDoublePercent(double value, int digits = 0)
        {
            double convertedValue = Math.Round(value / 100, digits, MidpointRounding.AwayFromZero);
            return convertedValue;
        }

        public static double ConvertRound(double value, int digits = 0)
        {
            double convertedValue = Math.Round(value, digits, MidpointRounding.AwayFromZero);
            return convertedValue;
        }

        public static double ConvertPercentDouble(double value, int digits = 0)
        {
            double convertedValue = Math.Round(value * 100, digits, MidpointRounding.AwayFromZero);
            return convertedValue;
        }

        public static double GetPercent(this double correctCount, double correctTotal, int digits = 0)
        {
            return correctTotal > 0 ? ConvertPercentDouble(correctCount / correctTotal, digits) : default;
        }

        public static double GetPercent(this int correctCount, int correctTotal, int digits = 0)
        {
            return correctTotal > 0 ? ConvertPercentDouble((double)correctCount / correctTotal, digits) : default;
        }

        public static double GetScore(this int correctCount, int correctTotal)
        {
            return correctTotal > 0 ? ((double)correctCount / correctTotal) * 10 : default;
        }

        public static long CalculateAverage(ICollection<long> secondsList)
        {
            if (secondsList == null || secondsList.Count == 0)
            {
                return 0;
            }

            long totalSeconds = secondsList.Sum();
            int count = secondsList.Count;

            return totalSeconds / count;
        }

        public static double GetTimeSkill(this EnumCourseSkill skill, string? fileAudio)
        {
            var timeAudio = MediaHelper.GetMediaDurationAsync(fileAudio) ?? default;
            if (skill == EnumCourseSkill.Reading || skill == EnumCourseSkill.Writing)
            {
                return SectionGroupIELST.ExecutionTimeReading;
            }
            else if (skill == EnumCourseSkill.Listening)
            {
                return timeAudio + SectionGroupIELST.AdditionalTimeListening;
            }
            else if (skill == EnumCourseSkill.Speaking && !string.IsNullOrEmpty(fileAudio))
            {
                return timeAudio; //thời gian audio
            }
            return default;
        }
    }
}
