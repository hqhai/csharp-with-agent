// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.EntityModels.Configs
{
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.Shared.Enums;

    public class ExamBandScoreSkill
    {
        public EnumCourseSkill CourseSkill { get; set; }
        public EnumExamPracticeType Type { get; set; }
        public IList<ExamBandScore> ExamBandScores { get; set; } = new List<ExamBandScore>();
    }
}
