// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Common.Helpers
{
    using Fsel.Common.Enums;

    public static class EnumSourceHelper
    {
        public static IList<EnumCourseLevel> GetEnumCourseLevels(this EnumCourseType? courseType)
        {
            var courseLevels = new List<EnumCourseLevel>();
            if (courseType == null)
            {
                return courseLevels;
            }

            switch (courseType)
            {
                case EnumCourseType.Academy:
                    courseLevels.Add(EnumCourseLevel.A2);
                    courseLevels.Add(EnumCourseLevel.B1);
                    courseLevels.Add(EnumCourseLevel.PlusB1);
                    courseLevels.Add(EnumCourseLevel.B2);
                    courseLevels.Add(EnumCourseLevel.C1);
                    break;

                case EnumCourseType.Ielts:
                    courseLevels.Add(EnumCourseLevel.Plus4);
                    courseLevels.Add(EnumCourseLevel.Plus5);
                    courseLevels.Add(EnumCourseLevel.Plus6);
                    courseLevels.Add(EnumCourseLevel.Plus7);
                    break;
            }

            return courseLevels;
        }

        public static IList<string> GetListCourseLevels(this EnumCourseType? courseType)
        {
            return GetEnumCourseLevels(courseType).Select(x => x.ToString()).ToList();
        }
    }
}
