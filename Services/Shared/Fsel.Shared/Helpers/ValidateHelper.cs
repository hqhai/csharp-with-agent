// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels.EntityModels;

    public static class ValidateHelper
    {
        public static bool HasRemainingAttempts(this UserCourseSettingModel? userCourseSetting)
        {
            return userCourseSetting.GetPropValue<int>("Value") > 0;
        }

        public static bool HasRemainingAttempts(this IList<UserCourseSettingModel>? userCourseSettings, EnumUserCourseType? type = null, EnumCourseLevel? level = null)
        {
            var userCourseSetting = userCourseSettings?.Where(x => !level.HasValue || x.GetPropValue<EnumCourseLevel>("CourseLevel") == level)
                                                             .FirstOrDefault(x => x.GetPropValue<EnumUserCourseType>("Type") == type);
            return userCourseSetting == null || userCourseSetting.HasRemainingAttempts();
        }
    }
}
