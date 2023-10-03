// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    public class NumberTokenConfig
    {
        public NumberTokenConfig(int levelOfGift, int numberOfToken)
        {
            LevelOfGift = levelOfGift;
            NumberOfToken = numberOfToken;
        }

        public int LevelOfGift { get; set; }
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

        public static int GetNumberToken(this int levelOfGift)
        {
            var config = s_numberTokenConfigs.OrderBy(x => x.LevelOfGift).FirstOrDefault(x => x.LevelOfGift == levelOfGift);
            if (config != null)
            {
                return config.NumberOfToken;
            }
            return default;
        }

        public static int GetNumbersToken(IList<int>? levelOfGifts)
        {
            if (levelOfGifts != null && levelOfGifts.Any())
            {
                return s_numberTokenConfigs.Where(x => levelOfGifts.Contains(x.LevelOfGift)).Sum(x => x.NumberOfToken);
            }

            return default;
        }
    }
}
