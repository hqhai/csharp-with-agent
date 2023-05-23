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
    }
}
