// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    public class NumberTokenConfig
    {
        public NumberTokenConfig(int number, int numberOfToken, bool isArmorialReceive)
        {
            Number = number;
            NumberOfToken = numberOfToken;
            IsArmorialReceive = isArmorialReceive;
        }

        public int Number { get; set; }
        public int NumberOfToken { get; set; }
        public bool IsArmorialReceive { get; set; }
    }

    public static class NumberTokenHelper
    {
        private static IList<NumberTokenConfig> s_numberTokenConfigs = new List<NumberTokenConfig>
        {
            new NumberTokenConfig(1, 1,false),
            new NumberTokenConfig(2, 3,false),
            new NumberTokenConfig(3, 10,true),
        };

        public static (int, bool) GetNumberToken(this int? number)
        {
            var config = s_numberTokenConfigs.OrderBy(x => x.Number).FirstOrDefault(x => x.Number == number);
            if (config != null)
            {
                return (config.NumberOfToken, config.IsArmorialReceive);
            }
            return default;
        }
    }
}
