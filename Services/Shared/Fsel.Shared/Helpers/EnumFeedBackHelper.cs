// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using Fsel.Shared.Enums;

    public static class EnumFeedBackHelper
    {
        private static IList<KeyValuePair<EnumFeedBackNegative, EnumFeedBackPositive>> s_feedBack = new List<KeyValuePair<EnumFeedBackNegative, EnumFeedBackPositive>>
        {
            new KeyValuePair<EnumFeedBackNegative, EnumFeedBackPositive>(EnumFeedBackNegative.NotUseful, EnumFeedBackPositive.Userful),
            new KeyValuePair<EnumFeedBackNegative, EnumFeedBackPositive>(EnumFeedBackNegative.HardToUnderStand, EnumFeedBackPositive.EasyToUnderStand),
            new KeyValuePair<EnumFeedBackNegative, EnumFeedBackPositive>(EnumFeedBackNegative.NotAccurate, EnumFeedBackPositive.Accurate),
            new KeyValuePair<EnumFeedBackNegative, EnumFeedBackPositive>(EnumFeedBackNegative.Slow, EnumFeedBackPositive.Fast),
        };

        public static bool IsCheckFeedBack(IList<EnumFeedBackNegative>? enumFeedBackNegatives, IList<EnumFeedBackPositive>? enumFeedBackPositives)
        {
            if (enumFeedBackPositives?.Count > 0)
            {
                foreach (var item in enumFeedBackPositives)
                {
                    var e = s_feedBack.FirstOrDefault(x => x.Value == item).Key;
                    if (enumFeedBackNegatives?.Count > 0 && enumFeedBackNegatives.Any(x => x == e))
                    {
                        return false;
                    }
                }
            }
            else if (enumFeedBackNegatives?.Count > 0)
            {
                foreach (var item in enumFeedBackNegatives)
                {
                    var e = s_feedBack.FirstOrDefault(x => x.Key == item).Value;
                    if (enumFeedBackPositives?.Count > 0 && enumFeedBackPositives.Any(x => x == e))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
