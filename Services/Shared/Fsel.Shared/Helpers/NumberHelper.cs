// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    public static class NumberHelper
    {
        public static double RoundNumberDouble(double number)
        {
            if (number < 0)
            {
                number *= 100;
            }
            double decimalPart = number % 1;

            if (decimalPart == 0.25)
            {
                return Math.Floor(number);
            }
            else if (decimalPart == 0.75)
            {
                return Math.Floor(number) + 0.5;
            }
            else
            {
                return Math.Round(number, 1, MidpointRounding.AwayFromZero);
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

        public static double ConvertDouble(double value)
        {
            double convertedValue = Math.Round(value, 2, MidpointRounding.AwayFromZero);
            return convertedValue;
        }

        public static double ConvertRound(double value)
        {
            double convertedValue = Math.Round(value, 0, MidpointRounding.AwayFromZero);
            return convertedValue;
        }

        public static double ConvertPercentDouble(double value)
        {
            double convertedValue = Math.Round(value * 100, 0, MidpointRounding.AwayFromZero);
            return convertedValue;
        }
    }
}
