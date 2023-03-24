// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Common.Helpers
{
    using System.ComponentModel;
    using Fsel.Common.Enums;

    public static class EnumHelper
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
                    courseLevels.Add(EnumCourseLevel.B1Plus);
                    courseLevels.Add(EnumCourseLevel.B2);
                    courseLevels.Add(EnumCourseLevel.C1);
                    break;

                case EnumCourseType.Ielts:
                    courseLevels.Add(EnumCourseLevel.RFE);
                    courseLevels.Add(EnumCourseLevel.MS1);
                    courseLevels.Add(EnumCourseLevel.MS2);
                    courseLevels.Add(EnumCourseLevel.MS3);
                    break;
            }

            return courseLevels;
        }

        public static string? GetClassCodeByEnumCourseLevel(this EnumCourseLevel? enumCourseLevel)
        {
            var classCode = "";
            switch (enumCourseLevel)
            {
                case EnumCourseLevel.A2:
                    classCode = "A";
                    break;

                case EnumCourseLevel.B1:
                    classCode = EnumCourseLevel.B1.ToString();
                    break;

                case EnumCourseLevel.B1Plus:
                    classCode = "B1+";
                    break;

                case EnumCourseLevel.B2:
                    classCode = EnumCourseLevel.B2.ToString();
                    break;

                case EnumCourseLevel.C1:
                    classCode = EnumCourseLevel.C1.ToString();
                    break;

                case EnumCourseLevel.RFE:
                    classCode = EnumCourseLevel.RFE.ToString();
                    break;

                case EnumCourseLevel.MS1:
                    classCode = EnumCourseLevel.MS1.ToString();
                    break;

                case EnumCourseLevel.MS2:
                    classCode = EnumCourseLevel.MS2.ToString();
                    break;

                case EnumCourseLevel.MS3:
                    classCode = EnumCourseLevel.MS3.ToString();
                    break;
            }
            return classCode;
        }

        public static IList<string> GetListCourseLevels(this EnumCourseType? courseType)
        {
            return GetEnumCourseLevels(courseType).Select(x => x.ToString()).ToList();
        }

        public static string? GetDescription(this Enum value)
        {
            if (value == null)
            { return default; }
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            if (name != null)
            {
                var field = type.GetField(name);
                if (field != null)
                { var attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute; if (attr != null) { return attr.Description; } }
            }
            return null;
        }
    }
}
