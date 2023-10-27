// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Common.Helpers
{
    using System.ComponentModel;
    using Fsel.Common.Enums;

    public static class EnumHelper
    {
        private static IList<KeyValuePair<EnumCourseType, EnumCourseLevel>> _courseTypeLevel = new List<KeyValuePair<EnumCourseType, EnumCourseLevel>>
        {
            new KeyValuePair<EnumCourseType, EnumCourseLevel>(EnumCourseType.Academy, EnumCourseLevel.A2),
            new KeyValuePair<EnumCourseType, EnumCourseLevel>(EnumCourseType.Academy, EnumCourseLevel.B1),
            new KeyValuePair<EnumCourseType, EnumCourseLevel>(EnumCourseType.Academy, EnumCourseLevel.B1Plus),
            new KeyValuePair<EnumCourseType, EnumCourseLevel>(EnumCourseType.Academy, EnumCourseLevel.B2),
            new KeyValuePair<EnumCourseType, EnumCourseLevel>(EnumCourseType.Academy, EnumCourseLevel.C1),
            new KeyValuePair<EnumCourseType, EnumCourseLevel>(EnumCourseType.Ielts, EnumCourseLevel.RFE),
            new KeyValuePair<EnumCourseType, EnumCourseLevel>(EnumCourseType.Ielts, EnumCourseLevel.MS1),
            new KeyValuePair<EnumCourseType, EnumCourseLevel>(EnumCourseType.Ielts, EnumCourseLevel.MS2),
            new KeyValuePair<EnumCourseType, EnumCourseLevel>(EnumCourseType.Ielts, EnumCourseLevel.MS3),
        };

        public static EnumCourseType GetEnumCourseType(this EnumCourseLevel courseLevel)
        {
            return _courseTypeLevel.FirstOrDefault(x => x.Value == courseLevel).Key;
        }

        public static IList<EnumCourseLevel> GetEnumCourseLevels(this EnumCourseType? courseType)
        {
            var courseLevels = new List<EnumCourseLevel>();
            if (courseType == null)
            {
                return courseLevels;
            }

            courseLevels = _courseTypeLevel.Where(x => x.Key == courseType).Select(x => x.Value).ToList();

            return courseLevels;
        }

        public static string? GetCodeByEnumCourseLevel(this EnumCourseLevel? enumCourseLevel)
        {
            var classCode = "";
            switch (enumCourseLevel)
            {
                case EnumCourseLevel.B1Plus:
                    classCode = "B1+";
                    break;

                default:
                    classCode = enumCourseLevel.ToString();
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
