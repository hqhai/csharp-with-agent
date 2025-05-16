// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Common
{
    using Fsel.Common.Helpers;
    using Fsel.ExamPractice.Domain.Enums;

    public static class EnumHelper
    {
        private static IList<KeyValuePair<EnumExamPracticeType, EnumExamPracticeSubType>> s_examPractice = new List<KeyValuePair<EnumExamPracticeType, EnumExamPracticeSubType>>
        {
            new KeyValuePair<EnumExamPracticeType, EnumExamPracticeSubType>(EnumExamPracticeType.IELTS, EnumExamPracticeSubType.FullMockTest),
            new KeyValuePair<EnumExamPracticeType, EnumExamPracticeSubType>(EnumExamPracticeType.IELTS, EnumExamPracticeSubType.SkillMockTest),
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
    }
}
