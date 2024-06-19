// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using Fsel.Shared.Enums;

    public static class EnumFocusModeHelper
    {
        private static IList<KeyValuePair<double, EnumTokenMission>> s_tokenMission = new List<KeyValuePair<double, EnumTokenMission>>
        {
            new KeyValuePair<double, EnumTokenMission>(900, EnumTokenMission.FocusModeFifteenMinutes),
            new KeyValuePair<double, EnumTokenMission>(1800, EnumTokenMission.FocusModeThirtyMinutes),
            new KeyValuePair<double, EnumTokenMission>(2700, EnumTokenMission.FocusModeFortyFiveMinutes),
            new KeyValuePair<double, EnumTokenMission>(3600, EnumTokenMission.FocusModeSixtyMinutes),
            new KeyValuePair<double, EnumTokenMission>(5400, EnumTokenMission.FocusModeNinetyMinutes),
        };

        public static EnumTokenMission GetEnumTokenMission(this double value)
        {
            return s_tokenMission.FirstOrDefault(x => x.Key == value).Value;
        }
    }
}
