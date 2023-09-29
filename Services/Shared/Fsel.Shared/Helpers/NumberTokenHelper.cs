// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    public class NumberTokenConfig
    {
        public NumberTokenConfig(int number, int numberOfToken)
        {
            Number = number;
            NumberOfToken = numberOfToken;
        }

        public int Number { get; set; }
        public int NumberOfToken { get; set; }
    }

    public static class NumberTokenHelper
    {
        private static IList<NumberTokenConfig> s_numberTokenConfigs = new List<NumberTokenConfig>
        {
            new NumberTokenConfig(1, 1),
            new NumberTokenConfig(2, 3),
            new NumberTokenConfig(3, 10),
        };

        public static int GetNumberToken(this int number)
        {
            var config = s_numberTokenConfigs.OrderBy(x => x.Number).FirstOrDefault(x => x.Number == number);
            if (config != null)
            {
                return config.NumberOfToken;
            }
            return default;
        }

        public static int GetNumbersToken(IList<int>? numbers)
        {
            if (numbers != null && numbers.Any())
            {
                return s_numberTokenConfigs.Where(x => numbers.Contains(x.Number)).Sum(x => x.NumberOfToken);
            }

            return default;
        }
    }
}
