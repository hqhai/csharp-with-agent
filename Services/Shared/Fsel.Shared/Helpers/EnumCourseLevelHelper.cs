// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using Fsel.Shared.Enums;

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
            new KeyValuePair<EnumCourseType, EnumCourseLevel>(EnumCourseType.Ielts, EnumCourseLevel.RFE),
            new KeyValuePair<EnumCourseType, EnumCourseLevel>(EnumCourseType.Ielts, EnumCourseLevel.MS1),
            new KeyValuePair<EnumCourseType, EnumCourseLevel>(EnumCourseType.Ielts, EnumCourseLevel.MS2),
            new KeyValuePair<EnumCourseType, EnumCourseLevel>(EnumCourseType.Ielts, EnumCourseLevel.MS3),
        };

        private static IList<KeyValuePair<EnumPlacementTestLevel, EnumCourseSkill>> s_placementTestTypeLevel = new List<KeyValuePair<EnumPlacementTestLevel, EnumCourseSkill>>
        {
            new KeyValuePair<EnumPlacementTestLevel, EnumCourseSkill>(EnumPlacementTestLevel.A1, EnumCourseSkill.Reading),
            new KeyValuePair<EnumPlacementTestLevel, EnumCourseSkill>(EnumPlacementTestLevel.A1, EnumCourseSkill.Listening),
            new KeyValuePair<EnumPlacementTestLevel, EnumCourseSkill>(EnumPlacementTestLevel.A1, EnumCourseSkill.Vocabulary),
            new KeyValuePair<EnumPlacementTestLevel, EnumCourseSkill>(EnumPlacementTestLevel.A1, EnumCourseSkill.Grammar),
            new KeyValuePair<EnumPlacementTestLevel, EnumCourseSkill>(EnumPlacementTestLevel.A2, EnumCourseSkill.Reading),
            new KeyValuePair<EnumPlacementTestLevel, EnumCourseSkill>(EnumPlacementTestLevel.A2, EnumCourseSkill.Listening),
            new KeyValuePair<EnumPlacementTestLevel, EnumCourseSkill>(EnumPlacementTestLevel.A2, EnumCourseSkill.Vocabulary),
            new KeyValuePair<EnumPlacementTestLevel, EnumCourseSkill>(EnumPlacementTestLevel.A2, EnumCourseSkill.Grammar),
            new KeyValuePair<EnumPlacementTestLevel, EnumCourseSkill>(EnumPlacementTestLevel.B1, EnumCourseSkill.Reading),
            new KeyValuePair<EnumPlacementTestLevel, EnumCourseSkill>(EnumPlacementTestLevel.B1, EnumCourseSkill.Listening),
            new KeyValuePair<EnumPlacementTestLevel, EnumCourseSkill>(EnumPlacementTestLevel.B1, EnumCourseSkill.Vocabulary),
            new KeyValuePair<EnumPlacementTestLevel, EnumCourseSkill>(EnumPlacementTestLevel.B1, EnumCourseSkill.Grammar),
            new KeyValuePair<EnumPlacementTestLevel, EnumCourseSkill>(EnumPlacementTestLevel.B1Plus, EnumCourseSkill.Reading),
            new KeyValuePair<EnumPlacementTestLevel, EnumCourseSkill>(EnumPlacementTestLevel.B1Plus, EnumCourseSkill.Listening),
            new KeyValuePair<EnumPlacementTestLevel, EnumCourseSkill>(EnumPlacementTestLevel.B1Plus, EnumCourseSkill.Vocabulary),
            new KeyValuePair<EnumPlacementTestLevel, EnumCourseSkill>(EnumPlacementTestLevel.B1Plus, EnumCourseSkill.Grammar),
            new KeyValuePair<EnumPlacementTestLevel, EnumCourseSkill>(EnumPlacementTestLevel.B2, EnumCourseSkill.Reading),
            new KeyValuePair<EnumPlacementTestLevel, EnumCourseSkill>(EnumPlacementTestLevel.B2, EnumCourseSkill.Listening),
            new KeyValuePair<EnumPlacementTestLevel, EnumCourseSkill>(EnumPlacementTestLevel.B2, EnumCourseSkill.Vocabulary),
            new KeyValuePair<EnumPlacementTestLevel, EnumCourseSkill>(EnumPlacementTestLevel.B2, EnumCourseSkill.Grammar),
            new KeyValuePair<EnumPlacementTestLevel, EnumCourseSkill>(EnumPlacementTestLevel.C1, EnumCourseSkill.Reading),
            new KeyValuePair<EnumPlacementTestLevel, EnumCourseSkill>(EnumPlacementTestLevel.C1, EnumCourseSkill.Listening),
            new KeyValuePair<EnumPlacementTestLevel, EnumCourseSkill>(EnumPlacementTestLevel.C1, EnumCourseSkill.Vocabulary),
            new KeyValuePair<EnumPlacementTestLevel, EnumCourseSkill>(EnumPlacementTestLevel.C1, EnumCourseSkill.Grammar),
            new KeyValuePair<EnumPlacementTestLevel, EnumCourseSkill>(EnumPlacementTestLevel.IELTS, EnumCourseSkill.Reading),
            new KeyValuePair<EnumPlacementTestLevel, EnumCourseSkill>(EnumPlacementTestLevel.IELTS, EnumCourseSkill.Listening),

        };

        public static EnumCourseType GetEnumCourseType(this EnumCourseLevel courseLevel)
        {
            return s_courseTypeLevel.FirstOrDefault(x => x.Value == courseLevel).Key;
        }

        public static object GetEnumPlacementTestSkills()
        {
            return s_placementTestTypeLevel.GroupBy(x => x.Key).Select(x => new
            {
                CourseType = x.Key,
                CourseSkills = x.Select(n => n.Value).ToArray()
            });
        }

        public static object GetEnumCourseLevels()
        {
            return s_courseTypeLevel.GroupBy(x => x.Key).Select(x => new
            {
                CourseType = x.Key,
                CourseLevels = x.Select(n => n.Value).ToArray()
            });
        }


        public static IList<EnumCourseLevel> GetEnumCourseLevels(this EnumCourseType? courseType)
        {
            var courseLevels = new List<EnumCourseLevel>();
            if (courseType == null)
            {
                courseLevels = s_courseTypeLevel.Select(x => x.Value).ToList();
            }
            else
            {
                courseLevels = s_courseTypeLevel.Where(x => x.Key == courseType).Select(x => x.Value).ToList();
            }

            return courseLevels;
        }

        public static string? GetCodeByEnumCourseLevel(this EnumCourseLevel? enumCourseLevel)
        {
            string? classCode;
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
    }
}
