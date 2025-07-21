// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using static Fsel.Shared.Constants.ValueSettings;

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
            new KeyValuePair<EnumCourseType, EnumCourseLevel>(EnumCourseType.EnglishFoundation, EnumCourseLevel.EFA1),
            new KeyValuePair<EnumCourseType, EnumCourseLevel>(EnumCourseType.EnglishFoundation, EnumCourseLevel.EFA2),
            new KeyValuePair<EnumCourseType, EnumCourseLevel>(EnumCourseType.EnglishFoundation, EnumCourseLevel.EFB1),
        };

        private static Dictionary<EnumCourseLevel, EnumCourseLevel> s_levelMapping = new Dictionary<EnumCourseLevel, EnumCourseLevel>
        {
            { EnumCourseLevel.B1Plus, EnumCourseLevel.MS1 },
            { EnumCourseLevel.B2, EnumCourseLevel.MS2 },
            { EnumCourseLevel.C1, EnumCourseLevel.MS3 },
            { EnumCourseLevel.A1, EnumCourseLevel.EFA1 },
            { EnumCourseLevel.A2, EnumCourseLevel.EFA2 },
            { EnumCourseLevel.B1, EnumCourseLevel.EFB1 }
        };

        public static EnumSkillLevel GetSkillLevel(EnumCourseLevel studentLevel, EnumCourseLevel courseLevel, bool? isStudentsAchieveScore)
        {
            var courseLevels = s_courseTypeLevel.Where(x => x.Key == courseLevel.GetEnumCourseType()).Select(x => x.Value).ToList();
            var skillLevels = ConvertHelper.EnumToList<EnumSkillLevel>();
            var skillLevel = EnumSkillLevel.Beginner;
            if (courseLevel.GetEnumCourseType() == EnumCourseType.Academic && studentLevel.GetEnumCourseType() == EnumCourseType.Academic)
            {
                if (studentLevel == courseLevel)
                {
                    return skillLevel;
                }
                var indexDistance = courseLevels.IndexOf(courseLevel) - courseLevels.IndexOf(studentLevel);
                return indexDistance > 1 ? EnumSkillLevel.Advanced : EnumSkillLevel.Intermediate;
            }
            else
            {
                var listLevelAca = ConvertHelper.Deserialize<List<LevelDtoModel>>(EnumCourseType.Academic.GetListCourseLevels(studentLevel, isStudentsAchieveScore));
                if (listLevelAca == null)
                {
                    return skillLevel;
                }
                foreach (var item in listLevelAca)
                {
                    item.SkillLevel = skillLevels[listLevelAca.IndexOf(item)];
                }
                if (courseLevel.GetEnumCourseType() != EnumCourseType.Academic)
                {
                    courseLevel = s_levelMapping.FirstOrDefault(x => x.Value == courseLevel).Key;
                }
                return listLevelAca.FirstOrDefault(x => x.CourseLevel == courseLevel)?.SkillLevel ?? skillLevel;
            }
        }

        public static EnumCourseType GetEnumCourseType(this EnumCourseLevel? courseLevel)
        {
            return s_courseTypeLevel.FirstOrDefault(x => x.Value == courseLevel).Key;
        }

        public static EnumCourseType GetEnumCourseType(this EnumCourseLevel courseLevel)
        {
            return s_courseTypeLevel.FirstOrDefault(x => x.Value == courseLevel).Key;
        }

        public static EnumCourseLevel? GetLevelAcaToLevelIELTS(this EnumCourseLevel courseLevel)
        {
            if (s_levelMapping.Any(x => x.Value == courseLevel))
            {
                return s_levelMapping.FirstOrDefault(x => x.Value == courseLevel).Key;
            }
            return default;
        }

        public static EnumCourseLevel? GetLevelIELTSToAca(this EnumCourseLevel? courseLevel)
        {
            if (s_levelMapping.Any(x => x.Key == courseLevel))
            {
                return s_levelMapping.FirstOrDefault(x => x.Key == courseLevel).Value;
            }
            return default;
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
            return default;
        }

        public static IList<EnumCourseLevel> GetCourseLevels(this IList<EnumCourseType>? courseTypes, IList<EnumCourseLevel>? courseLevels = null)
        {
            var listCourseLevels = courseTypes?.SelectMany(x => GetEnumCourseLevels(x)).ToList() ?? new List<EnumCourseLevel>();
            if (courseLevels != null && courseLevels.Any())
            {
                listCourseLevels = courseLevels.ToList();
            }
            return listCourseLevels;
        }

        public static EnumPlacementTestLevel GetPlacementTestLevelByCourseLevel(this EnumCourseLevel level)
        {
            foreach (var item in ConvertHelper.EnumToList<EnumPlacementTestLevel>())
            {
                if (item.ToString() == level.ToString())
                {
                    return item;
                }
            }
            return EnumPlacementTestLevel.IELTS;
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

        public static IList<EnumCourseLevel> GetEnumCourseLevels(this EnumCourseType courseType)
        {
            return s_courseTypeLevel.Where(x => x.Key == courseType).Select(x => x.Value).ToList();
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

        public static bool CheckLevelByPass(this EnumCourseLevel? courseLevelStudent, EnumCourseLevel courseLevelChoose, bool? isDoneCourse)
        {
            var courseType = courseLevelChoose.GetEnumCourseType();
            var datas = courseType.GetListCourseLevels(courseLevelStudent ?? default, isDoneCourse);
            if (datas != null && datas is IList list)
            {
                var objects = list.Cast<object>().ToList();
                return objects.Any(x => x.GetPropValue<EnumCourseLevel>("CourseLevel") == courseLevelChoose);
            }
            return false;
        }

        private static EnumCourseLevel GetEnumCourseLevel(this EnumCourseType courseType, EnumCourseLevel courseLevel)
        {
            if (courseType == courseLevel.GetEnumCourseType())
            {
                return courseLevel;
            }
            if (courseLevel.GetEnumCourseType() == EnumCourseType.Academic)
            {
                return courseLevel;
            }
            return s_levelMapping.Where(x => x.Value == courseLevel).Select(x => x.Key).FirstOrDefault();
        }

        public static object? GetListCourseLevels(this EnumCourseType courseType, EnumCourseLevel courseLevel, bool? isCourseDoneAndAchieveGrade = null)
        {
            courseLevel = courseType.GetEnumCourseLevel(courseLevel);
            int index = (int)s_courseTypeLevel.FirstOrDefault(x => x.Key == courseLevel.GetEnumCourseType() && x.Value == courseLevel).Value;
            var relevantLevels = s_courseTypeLevel
                .Where((x, i) => isCourseDoneAndAchieveGrade.HasValue ? isCourseDoneAndAchieveGrade.Value ? (i >= index && i <= index + 2) : (i >= index && i <= index + 1) : (i >= index - 1 && i <= index + 1))
                .Where(x => x.Key == courseLevel.GetEnumCourseType())
                .ToList();
            if (relevantLevels == null || !relevantLevels.Any())
            {
                return default;
            }
            var listCourselevel = relevantLevels.Select(x => x.Value).ToList();
            var courseLevels = listCourselevel;
            if (courseType == EnumCourseType.Academic)
            {
                if (courseLevel.GetEnumCourseType() == EnumCourseType.Ielts)
                {
                    courseLevels = s_levelMapping.Where(x => listCourselevel.Contains(x.Value)).Select(x => x.Key).ToList();
                    return s_courseTypeLevel
                    .Where(x => x.Key == courseType && courseLevels.Contains(x.Value))
                    .Select(x => new
                    {
                        CourseLevel = x.Value,
                    }).ToList();
                }
                return relevantLevels.Select(x => new
                {
                    CourseLevel = x.Value,
                }).ToList();
            }
            else
            {
                if (courseLevel.GetEnumCourseType() == EnumCourseType.Academic)
                {
                    courseLevels = s_levelMapping.Where(x => listCourselevel.Contains(x.Key)).Select(x => x.Value).ToList();
                }
                return s_courseTypeLevel
                    .Where(x => x.Key == courseType && courseLevels.Contains(x.Value))
                    .Select(x => new
                    {
                        CourseLevel = x.Value,
                    }).ToList();
            }
        }

        public static bool IsCheckCourseLevel(this EnumCourseLevel courseLevelSelected, EnumCourseLevel courseLevel)
        {
            int index = (int)s_courseTypeLevel.FirstOrDefault(x => x.Key == courseLevel.GetEnumCourseType() && x.Value == courseLevel).Value;
            var levels = s_courseTypeLevel
                        .Where((x, i) => i >= index - 1 && i <= index + 1 && x.Key == courseLevel.GetEnumCourseType())
                        .Select(x => x.Value)
                        .ToList();
            if (courseLevel.GetEnumCourseType() == courseLevelSelected.GetEnumCourseType())
            {
                return levels.Any(x => x == courseLevelSelected);
            }
            else
            {
                var courseLevelIELSTs = s_levelMapping.Where(x => levels.Contains(x.Key)).Select(x => x.Value).ToList();
                return courseLevelIELSTs.Any(x => x == courseLevelSelected);
            }
        }

        public static EnumCourseLevel GetEnumNextCourseLevel(this EnumCourseType? courseType, EnumCourseLevel courseLevel)
        {
            if (courseType == EnumCourseType.Academic && courseLevel != EnumCourseLevel.C1)
            {
                courseLevel += 1;
            }
            if (courseType == EnumCourseType.Ielts && courseLevel != EnumCourseLevel.MS3)
            {
                courseLevel += 1;
            }

            return courseLevel;
        }

        public static string GetCourseTitle(EnumCourseLevel courseLevel)
        {
            switch (courseLevel)
            {
                case EnumCourseLevel.A1:
                    return SendMailSetting.CourseA1Title;

                case EnumCourseLevel.A2:
                    return SendMailSetting.CourseA2Title;

                case EnumCourseLevel.B1:
                    return SendMailSetting.CourseB1Title;

                case EnumCourseLevel.EFA1:
                    return SendMailSetting.CourseA1Title;

                case EnumCourseLevel.EFA2:
                    return SendMailSetting.CourseA2Title;

                case EnumCourseLevel.EFB1:
                    return SendMailSetting.CourseB1Title;

                case EnumCourseLevel.B1Plus:
                    return SendMailSetting.CourseB1PlusTitle;

                case EnumCourseLevel.B2:
                    return SendMailSetting.CourseB2Title;

                case EnumCourseLevel.C1:
                    return SendMailSetting.CourseC1Title;

                case EnumCourseLevel.MS1:
                    return SendMailSetting.CourseMS1Title;

                case EnumCourseLevel.MS2:
                    return SendMailSetting.CourseMS2Title;

                case EnumCourseLevel.MS3:
                    return SendMailSetting.CourseMS3Title;

                default:
                    return string.Empty;
            }
        }

        public static string GetCourseInfo(EnumCourseLevel? courseLevel)
        {
            switch (courseLevel)
            {
                case null:
                    return SendMailSetting.CoursePreA1;

                case EnumCourseLevel.A1:
                    return SendMailSetting.CourseA1;

                case EnumCourseLevel.A2:
                    return SendMailSetting.CourseA2;

                case EnumCourseLevel.B1:
                    return SendMailSetting.CourseB1;

                case EnumCourseLevel.B1Plus:
                    return SendMailSetting.CourseB1Plus;

                case EnumCourseLevel.B2:
                    return SendMailSetting.CourseB2;

                case EnumCourseLevel.C1:
                    return SendMailSetting.CourseC1;

                case EnumCourseLevel.MS1:
                    return SendMailSetting.Mindset1;

                case EnumCourseLevel.MS2:
                    return SendMailSetting.Mindset2;

                default:
                    return SendMailSetting.Mindset3;
            }
        }

        public static string GetLevelPhoto(EnumCourseLevel courseLevel)
        {
            switch (courseLevel)
            {
                case EnumCourseLevel.EFA1:
                    return SendMailSetting.A1Photo;

                case EnumCourseLevel.EFA2:
                    return SendMailSetting.A2Photo;

                case EnumCourseLevel.EFB1:
                    return SendMailSetting.B1Photo;

                case EnumCourseLevel.A1:
                    return SendMailSetting.A1Photo;

                case EnumCourseLevel.A2:
                    return SendMailSetting.A2Photo;

                case EnumCourseLevel.B1:
                    return SendMailSetting.B1Photo;

                case EnumCourseLevel.B1Plus:
                    return SendMailSetting.B1PlusPhoto;

                case EnumCourseLevel.B2:
                    return SendMailSetting.B2Photo;

                case EnumCourseLevel.C1:
                    return SendMailSetting.C1Photo;

                case EnumCourseLevel.MS1:
                    return SendMailSetting.MS1Photo;

                case EnumCourseLevel.MS2:
                    return SendMailSetting.MS2Photo;

                case EnumCourseLevel.MS3:
                    return SendMailSetting.MS3Photo;

                default:
                    return string.Empty;
            }
        }

        public static int GetTotalProgress(this EnumCourseType courseType)
        {
            switch (courseType)
            {
                case EnumCourseType.Academic:
                    return CourseProgressValue.ProgressAcademic;

                case EnumCourseType.Ielts:
                    return CourseProgressValue.ProgressIELTS;

                default:
                    return 0;
            }
        }
    }
}
