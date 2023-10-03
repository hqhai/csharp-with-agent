// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    public static class NumberTokenHelper
    {
        private static Dictionary<int, int> s_numberTokenConfigs = new Dictionary<int, int>
        {
            {1,1 }, {2,3 }, {3,10 }
        };

        public static int GetNumberToken(this int levelOfGift)
        {
            var config = s_numberTokenConfigs.OrderBy(x => x.Key).FirstOrDefault(x => x.Key == levelOfGift);
            return config.Value;
        }
    }
}
