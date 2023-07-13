// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using Fsel.Common.Helpers;
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

        public static EnumCourseType GetEnumCourseType(this EnumCourseLevel courseLevel)
        {
            return s_courseTypeLevel.FirstOrDefault(x => x.Value == courseLevel).Key;
        }

        public static object GetEnumPlacementTestSkills()
        {
            var results = new List<object>();
            foreach (var item in ConvertHelper.EnumToList<EnumPlacementTestLevel>())
            {
                var result = new
                {
                    LevelValue = item,
                    LevelName = item.GetDescription(),
                    Skills = new List<EnumCourseSkill> { EnumCourseSkill.Reading, EnumCourseSkill.Listening }
                };

                if (item != EnumPlacementTestLevel.IELTS)
                {
                    result.Skills.Add(EnumCourseSkill.Vocabulary);
                    result.Skills.Add(EnumCourseSkill.Grammar);
                }
                results.Add(result);
            }

            return results;
        }

        public static EnumCourseLevel GetCourseLevelByPlacementTestLevel(this EnumPlacementTestLevel level)
        {
            foreach (var item in ConvertHelper.EnumToList<EnumCourseLevel>())
            {
                if (item.ToString() == level.ToString())
                {
                    return item;
                }
            }
            return EnumCourseLevel.MS3;
        }

        public static object GetEnumCourseLevels()
        {
            return s_courseTypeLevel.GroupBy(x => x.Key).Select(x => new
            {
                CourseType = x.Key,
                CourseLevels = x.Select(n => n.Value).ToArray()
            });
        }
        public static EnumCourseLevel GetCourseLevelByPlacementTestLevel(this EnumPlacementTestLevel level)
        {
            foreach (var item in ConvertHelper.EnumToList<EnumCourseLevel>())
            {
                if (item.ToString() == level.ToString())
                {
                    return item;
                }
            }
            return EnumCourseLevel.MS3;
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
