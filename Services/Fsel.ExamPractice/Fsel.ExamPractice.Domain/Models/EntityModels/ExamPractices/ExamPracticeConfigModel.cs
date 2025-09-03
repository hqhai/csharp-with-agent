// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices
{
    using Fsel.Common.Models;

    public class ExamPracticeConfigModel
    {
        public IList<ExamPracticePartModel> ExamPracticeParts { get; set; } = new List<ExamPracticePartModel>();
        public IList<EnumModel> PracticeTimeLimitRules { get; set; } = new List<EnumModel>();
    }
}
