// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    public class NumberTokenConfig
    {
        public NumberTokenConfig(int levelOfGift, int numberOfToken, bool isArmorialReceive)
        {
            LevelOfGift = levelOfGift;
            NumberOfToken = numberOfToken;
            IsArmorialReceive = isArmorialReceive;
        }

        public int LevelOfGift { get; set; }
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

        public static (int, bool) GetNumberToken(this int? levelOfGift)
        {
            var config = s_numberTokenConfigs.OrderBy(x => x.LevelOfGift).FirstOrDefault(x => x.LevelOfGift == levelOfGift);
            if (config != null)
            {
                return (config.NumberOfToken, config.IsArmorialReceive);
            }
            return default;
        }
    }
}
