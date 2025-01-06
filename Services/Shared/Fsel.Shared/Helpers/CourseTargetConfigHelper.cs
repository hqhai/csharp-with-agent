// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using System.Collections.Generic;
    using Fsel.Shared.Models.ShareModels;

    public static class CourseTargetConfigHelper
    {
        public static IList<CourseTargetConfigModel> CourseTargetConfig(this IList<CourseTargetConfigModel> courseTargetConfigs, int totalLesson)
        {
            ArgumentNullException.ThrowIfNull(courseTargetConfigs);

            foreach (var courseTargetConfig in courseTargetConfigs)
            {
                double totalWeek = (totalLesson * courseTargetConfig.MaxHoursPerLesson) / (courseTargetConfig.LessonNumberPerWeek * courseTargetConfig.MaxHoursPerLesson);

                var numberMonth = totalWeek / 4;
                courseTargetConfig.Month = (int)Math.Round(numberMonth);
            }

            return courseTargetConfigs;
        }
    }
}
