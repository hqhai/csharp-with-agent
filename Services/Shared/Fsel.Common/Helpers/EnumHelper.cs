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

        public static string? GetTrainingCodeByEnumCourseLevel(this EnumCourseLevel? enumCourseLevel)
        {
            var trainingCode = "";
            switch (enumCourseLevel)
            {
                case EnumCourseLevel.A2:
                    trainingCode = "A2";
                    break;

                case EnumCourseLevel.B1:
                    trainingCode = EnumCourseLevel.B1.ToString();
                    break;

                case EnumCourseLevel.B1Plus:
                    trainingCode = "B1+";
                    break;

                case EnumCourseLevel.B2:
                    trainingCode = EnumCourseLevel.B2.ToString();
                    break;

                case EnumCourseLevel.C1:
                    trainingCode = EnumCourseLevel.C1.ToString();
                    break;

                case EnumCourseLevel.RFE:
                    trainingCode = EnumCourseLevel.RFE.ToString();
                    break;

                case EnumCourseLevel.MS1:
                    trainingCode = EnumCourseLevel.MS1.ToString();
                    break;

                case EnumCourseLevel.MS2:
                    trainingCode = EnumCourseLevel.MS2.ToString();
                    break;

                case EnumCourseLevel.MS3:
                    trainingCode = EnumCourseLevel.MS3.ToString();
                    break;
            }
            return trainingCode;
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
