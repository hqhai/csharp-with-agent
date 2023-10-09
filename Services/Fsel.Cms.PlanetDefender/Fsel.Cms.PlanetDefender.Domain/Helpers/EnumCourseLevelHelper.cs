// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using Fsel.Cms.PlanetDefender.Domain.Enums;

    public static class EnumCourseLevelHelper
    {
        private static IList<KeyValuePair<EnumCourseType, EnumCourseLevel>> s_courseTypeLevel = new List<KeyValuePair<EnumCourseType, EnumCourseLevel>>
        {
            new KeyValuePair<EnumCourseType, EnumCourseLevel>(EnumCourseType.Academic, EnumCourseLevel.A1),
            new KeyValuePair<EnumCourseType, EnumCourseLevel>(EnumCourseType.Academic, EnumCourseLevel.A2),
            new KeyValuePair<EnumCourseType, EnumCourseLevel>(EnumCourseType.Academic, EnumCourseLevel.B1),
            new KeyValuePair<EnumCourseType, EnumCourseLevel>(EnumCourseType.Academic, EnumCourseLevel.B1Plus),
            new KeyValuePair<EnumCourseType, EnumCourseLevel>(EnumCourseType.Academic, EnumCourseLevel.B2),
            new KeyValuePair<EnumCourseType, EnumCourseLevel>(EnumCourseType.Academic, EnumCourseLevel.C1),
            new KeyValuePair<EnumCourseType, EnumCourseLevel>(EnumCourseType.Ielts, EnumCourseLevel.MS1),
            new KeyValuePair<EnumCourseType, EnumCourseLevel>(EnumCourseType.Ielts, EnumCourseLevel.MS2),
            new KeyValuePair<EnumCourseType, EnumCourseLevel>(EnumCourseType.Ielts, EnumCourseLevel.MS3),
        };

        public static object GetEnumCourseLevels()
        {
            return s_courseTypeLevel.GroupBy(x => x.Key).Select(x => new
            {
                CourseType = x.Key,
                CourseLevels = x.Select(n => n.Value).ToArray()
            });
        }
    }
}
