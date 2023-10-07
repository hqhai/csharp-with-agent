// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    public static class NumberHelper
    {
        public static double RoundNumberDouble(double number)
        {
            double decimalPart = number % 1;

            if (decimalPart == 0.25)
            {
                return Math.Floor(number) + 0.5;
            }
            else if (decimalPart == 0.75)
            {
                return Math.Ceiling(number);
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
            double convertedValue = Math.Round(value / 100, 2);
            return convertedValue;
        }

        public static double ConvertDouble(double value)
        {
            double convertedValue = Math.Round(value, 2);
            return convertedValue;
        }

        public static double ConvertDoubleDecimal(double value)
        {
            double convertedValue = Math.Round(value, 0);
            return convertedValue;
        }

        public static double ConvertNumberOfStarDouble(double value)
        {
            if (value - Math.Floor(value) == 0.5)
            {
                value += 0.1;
            }
            double convertedValue = Math.Round(value, 0);
            return convertedValue;
        }

        public static double ConvertPercentDouble(double value)
        {
            double convertedValue = Math.Round(value * 100, 0);
            return convertedValue;
        }
    }
}
