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
            if ((enumFeedBackNegatives == null || enumFeedBackNegatives.Count == 0) &&
                (enumFeedBackPositives == null || enumFeedBackPositives.Count == 0))
            {
                return true;
            }

            var positiveSet = new HashSet<EnumFeedBackPositive>(enumFeedBackPositives ?? Enumerable.Empty<EnumFeedBackPositive>());
            var negativeSet = new HashSet<EnumFeedBackNegative>(enumFeedBackNegatives ?? Enumerable.Empty<EnumFeedBackNegative>());

            foreach (var feedbackPair in s_feedBack)
            {
                if (positiveSet.Contains(feedbackPair.Value) && negativeSet.Contains(feedbackPair.Key))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
