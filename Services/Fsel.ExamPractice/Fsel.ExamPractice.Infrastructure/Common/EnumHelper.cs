// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Common
{
    using Fsel.Common.Helpers;
    using Fsel.Common.Models;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
    using Fsel.Shared.Enums;

    public static class EnumHelper
    {
        #region s_examPractice

        private static IList<KeyValuePair<EnumExamPracticeType, EnumExamPracticeSubType>> s_examPractice = new List<KeyValuePair<EnumExamPracticeType, EnumExamPracticeSubType>>
        {
            new KeyValuePair<EnumExamPracticeType, EnumExamPracticeSubType>(EnumExamPracticeType.IELTS, EnumExamPracticeSubType.FullMockTest),
            new KeyValuePair<EnumExamPracticeType, EnumExamPracticeSubType>(EnumExamPracticeType.IELTS, EnumExamPracticeSubType.SkillMockTest),
            new KeyValuePair<EnumExamPracticeType, EnumExamPracticeSubType>(EnumExamPracticeType.Vstep, EnumExamPracticeSubType.SingleVstepSkill),
            new KeyValuePair<EnumExamPracticeType, EnumExamPracticeSubType>(EnumExamPracticeType.Vstep, EnumExamPracticeSubType.FullVstepSkill),
            new KeyValuePair<EnumExamPracticeType, EnumExamPracticeSubType>(EnumExamPracticeType.ExamPractice, EnumExamPracticeSubType.Practice),
            new KeyValuePair<EnumExamPracticeType, EnumExamPracticeSubType>(EnumExamPracticeType.ExamPractice, EnumExamPracticeSubType.UniversityEntrance),
            new KeyValuePair<EnumExamPracticeType, EnumExamPracticeSubType>(EnumExamPracticeType.ExamPractice, EnumExamPracticeSubType.HighschoolEntrance)
        };

        public static object GetExamPracticeSubTypes(this EnumExamPracticeType examPracticeType)
        {
            return s_examPractice.Where(x => x.Key == examPracticeType).Select(x => new
            {
                Value = x.Value,
                Description = x.Value.GetDescription()
            }).ToList();
        }

        public static IList<EnumExamPracticeSubType> GetSubTypes(this EnumExamPracticeType examPracticeType)
        {
            return s_examPractice.Where(x => x.Key == examPracticeType).Select(x => x.Value).ToList();
        }

        #endregion s_examPractice

        #region PracticeTimeLimitRules

        public static List<PracticeTimeLimitRuleModel> PracticeTimeLimitRules = new()
        {
            new PracticeTimeLimitRuleModel
            {
                Type = EnumExamPracticeType.IELTS,
                SubType = EnumExamPracticeSubType.FullMockTest,
            },
            new PracticeTimeLimitRuleModel
            {
                Type = EnumExamPracticeType.IELTS,
                SubType = EnumExamPracticeSubType.SkillMockTest,
                Skill = EnumCourseSkill.Reading,
                TimeLimits = new List<EnumPracticeTimeLimitOption>
                {
                    EnumPracticeTimeLimitOption.Minute30,
                    EnumPracticeTimeLimitOption.Minute45,
                    EnumPracticeTimeLimitOption.Minute60,
                    EnumPracticeTimeLimitOption.Minute75,
                    EnumPracticeTimeLimitOption.Minute90,
                    EnumPracticeTimeLimitOption.Unlimited
                }
            },
            new PracticeTimeLimitRuleModel
            {
                Type = EnumExamPracticeType.IELTS,
                SubType = EnumExamPracticeSubType.SkillMockTest,
                Skill = EnumCourseSkill.Writing,
                TimeLimits = new List<EnumPracticeTimeLimitOption>
                {
                    EnumPracticeTimeLimitOption.Minute30,
                    EnumPracticeTimeLimitOption.Minute45,
                    EnumPracticeTimeLimitOption.Minute60,
                    EnumPracticeTimeLimitOption.Minute75,
                    EnumPracticeTimeLimitOption.Minute90,
                    EnumPracticeTimeLimitOption.Unlimited
                }
            },
            new PracticeTimeLimitRuleModel
            {
                Type = EnumExamPracticeType.IELTS,
                SubType = EnumExamPracticeSubType.SkillMockTest,
                Skill = EnumCourseSkill.Speaking,
                TimeLimits = new List<EnumPracticeTimeLimitOption>
                {
                    EnumPracticeTimeLimitOption.ExamBased,
                    EnumPracticeTimeLimitOption.Unlimited
                }
            },
            new PracticeTimeLimitRuleModel
            {
                Type = EnumExamPracticeType.IELTS,
                SubType = EnumExamPracticeSubType.SkillMockTest,
                Skill = EnumCourseSkill.Listening,
                TimeLimits = new List<EnumPracticeTimeLimitOption>
                {
                    EnumPracticeTimeLimitOption.ExamBased,
                    EnumPracticeTimeLimitOption.Unlimited
                }
            },
            new PracticeTimeLimitRuleModel
            {
                Type = EnumExamPracticeType.ExamPractice,
                TimeLimits = new List<EnumPracticeTimeLimitOption>
                {
                    EnumPracticeTimeLimitOption.Minute30,
                    EnumPracticeTimeLimitOption.Minute45,
                    EnumPracticeTimeLimitOption.Minute60,
                    EnumPracticeTimeLimitOption.Minute75,
                    EnumPracticeTimeLimitOption.Minute90,
                    EnumPracticeTimeLimitOption.Unlimited
                }
            }
        };

        public static IList<EnumModel> GetEnumPracticeTimeLimits(this EnumExamPracticeType type, EnumExamPracticeSubType? subType, EnumCourseSkill? courseSkill)
        {
            var query = PracticeTimeLimitRules.Where(x => x.Type == type);
            if (type == EnumExamPracticeType.IELTS)
            {
                if (subType.HasValue)
                {
                    query = query.Where(x => x.SubType == subType.Value);
                }
                if (courseSkill.HasValue)
                {
                    query = query.Where(x => x.Skill == courseSkill.Value);
                }
            }

            return query.SelectMany(x => x.TimeLimits).Select(x => new EnumModel
            {
                Name = x.GetDescription(),
                Value = x.ToString(),
            }).ToList();
        }

        #endregion PracticeTimeLimitRules

        #region s_examPracticeTag

        private static IList<KeyValuePair<EnumExamPracticeType, ExamPracticeTagModel>> s_examPracticeTag = new List<KeyValuePair<EnumExamPracticeType, ExamPracticeTagModel>>
        {
            new KeyValuePair<EnumExamPracticeType, ExamPracticeTagModel>(EnumExamPracticeType.IELTS, new ExamPracticeTagModel
            {
                SubType = EnumExamPracticeSubType.FullMockTest,
                Description = EnumExamPracticeSubType.FullMockTest.GetDescription()
            }),
            new KeyValuePair<EnumExamPracticeType, ExamPracticeTagModel>(EnumExamPracticeType.IELTS,new ExamPracticeTagModel
            {
                SubType = EnumExamPracticeSubType.SkillMockTest,
                Description = EnumExamPracticeSubType.SkillMockTest.GetDescription(),
                CourseSkill = EnumCourseSkill.Reading
            }),
            new KeyValuePair<EnumExamPracticeType, ExamPracticeTagModel>(EnumExamPracticeType.IELTS,new ExamPracticeTagModel
            {
                SubType = EnumExamPracticeSubType.SkillMockTest,
                Description = EnumExamPracticeSubType.SkillMockTest.GetDescription(),
                CourseSkill = EnumCourseSkill.Listening
            }),
            new KeyValuePair<EnumExamPracticeType, ExamPracticeTagModel>(EnumExamPracticeType.IELTS,new ExamPracticeTagModel
            {
                SubType = EnumExamPracticeSubType.SkillMockTest,
                Description = EnumExamPracticeSubType.SkillMockTest.GetDescription(),
                CourseSkill = EnumCourseSkill.Writing
            }),
            new KeyValuePair<EnumExamPracticeType, ExamPracticeTagModel>(EnumExamPracticeType.IELTS,new ExamPracticeTagModel
            {
                SubType = EnumExamPracticeSubType.SkillMockTest,
                Description = EnumExamPracticeSubType.SkillMockTest.GetDescription(),
                CourseSkill = EnumCourseSkill.Speaking
            }),
            new KeyValuePair<EnumExamPracticeType, ExamPracticeTagModel>(EnumExamPracticeType.ExamPractice,new ExamPracticeTagModel
            {
                SubType = EnumExamPracticeSubType.UniversityEntrance,
                Description = EnumExamPracticeSubType.UniversityEntrance.GetDescription()
            }),
            new KeyValuePair<EnumExamPracticeType, ExamPracticeTagModel>(EnumExamPracticeType.ExamPractice,new ExamPracticeTagModel
            {
                SubType = EnumExamPracticeSubType.HighschoolEntrance,
                Description = EnumExamPracticeSubType.HighschoolEntrance.GetDescription()
            }),
            new KeyValuePair<EnumExamPracticeType, ExamPracticeTagModel>(EnumExamPracticeType.ExamPractice,new ExamPracticeTagModel
            {
                SubType = EnumExamPracticeSubType.Practice,
                Description = EnumExamPracticeSubType.Practice.GetDescription()
            }), new KeyValuePair<EnumExamPracticeType, ExamPracticeTagModel>(EnumExamPracticeType.Vstep, new ExamPracticeTagModel
            {
                SubType = EnumExamPracticeSubType.FullVstepSkill,
                Description = EnumExamPracticeSubType.FullVstepSkill.GetDescription()
            }),
            new KeyValuePair<EnumExamPracticeType, ExamPracticeTagModel>(EnumExamPracticeType.Vstep,new ExamPracticeTagModel
            {
                SubType = EnumExamPracticeSubType.SingleVstepSkill,
                Description = EnumExamPracticeSubType.SingleVstepSkill.GetDescription(),
                CourseSkill = EnumCourseSkill.Reading
            }),
            new KeyValuePair<EnumExamPracticeType, ExamPracticeTagModel>(EnumExamPracticeType.Vstep,new ExamPracticeTagModel
            {
                SubType = EnumExamPracticeSubType.SingleVstepSkill,
                Description = EnumExamPracticeSubType.SingleVstepSkill.GetDescription(),
                CourseSkill = EnumCourseSkill.Listening
            }),
            new KeyValuePair<EnumExamPracticeType, ExamPracticeTagModel>(EnumExamPracticeType.Vstep,new ExamPracticeTagModel
            {
                SubType = EnumExamPracticeSubType.SingleVstepSkill,
                Description = EnumExamPracticeSubType.SingleVstepSkill.GetDescription(),
                CourseSkill = EnumCourseSkill.Writing
            }),
            new KeyValuePair<EnumExamPracticeType, ExamPracticeTagModel>(EnumExamPracticeType.Vstep,new ExamPracticeTagModel
            {
                SubType = EnumExamPracticeSubType.SingleVstepSkill,
                Description = EnumExamPracticeSubType.SingleVstepSkill.GetDescription(),
                CourseSkill = EnumCourseSkill.Speaking
            }),
        };

        public static IList<ExamPracticeTagModel> GetTags(this EnumExamPracticeType examPracticeType)
        {
            return s_examPracticeTag.Where(x => x.Key == examPracticeType).Select(x => x.Value).ToList();
        }

        #endregion s_examPracticeTag

        public static EnumCorrectStatus? GetStatus(BaseAnswer? baseAnswer, bool isDone)
        {
            return baseAnswer != null ? (isDone ? (baseAnswer.IsCorrect == true ? EnumCorrectStatus.Correct : EnumCorrectStatus.Fail) : EnumCorrectStatus.Process) : null;
        }
    }
}
