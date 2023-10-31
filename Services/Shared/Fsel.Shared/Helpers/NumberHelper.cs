// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    public static class NumberHelper
    {
        public static double RoundNumberDouble(double number, bool roundUp = false)
        {
            if (roundUp)
            {
                return Math.Ceiling(number * 2) / 2;
            }
            else
            {
                return Math.Floor(number * 2) / 2;
            }
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

        public static double ConvertDoublePercent(double value)
        {
            double convertedValue = Math.Round(value / 100, 2, MidpointRounding.AwayFromZero);
            return convertedValue;
        }

        public static double ConvertRound(double value, int digits = 0)
        {
            double convertedValue = Math.Round(value, digits, MidpointRounding.AwayFromZero);
            return convertedValue;
        }

        public static double ConvertPercentDouble(double value)
        {
            double convertedValue = Math.Round(value * 100, 0, MidpointRounding.AwayFromZero);
            return convertedValue;
        }

        public static double GetPercent(this double correctCount, double correctTotal)
        {
            return correctTotal > 0 ? ConvertPercentDouble(correctCount / correctTotal) : default;
        }

        public static double GetPercent(this int correctCount, int correctTotal)
        {
            return correctTotal > 0 ? ConvertPercentDouble((double)correctCount / correctTotal) : default;
        }
    }
}
