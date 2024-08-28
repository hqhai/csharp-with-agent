// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using System.Collections;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;

    public static class ValidateHelper
    {
        public static bool IsValidValue(this object? data, EnumUserCourseType? type = null, EnumCourseLevel? level = null)
        {
            if (data is IList list)
            {
                var userCourseSettings = list.OfType<object>().ToList();
                var userCourseSetting = userCourseSettings
                    .Where(x => !level.HasValue || x.GetPropValue<EnumCourseLevel>("CourseLevel") == level)
                    .FirstOrDefault(x => x.GetPropValue<EnumUserCourseType>("Type") == type);
                return userCourseSetting == null || userCourseSetting.GetPropValue<int>("Value") > 0;
            }
            return data.GetPropValue<int>("Value") > 0;
        }
    }
}
