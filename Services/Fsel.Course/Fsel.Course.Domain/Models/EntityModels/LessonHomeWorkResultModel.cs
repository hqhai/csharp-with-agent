// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Helpers;

    public class LessonHomeWorkResultModel : HomeWorkModel
    {
        public Guid? LessonModuleId { get; set; }
        public long QuestionCompleted { get; set; }
        public long QuestionTotal { get; set; }

        public double QuestionPercent
        {
            get
            {
                return QuestionTotal > 0 ? NumberHelper.GetPercent(QuestionCompleted, QuestionTotal) : default;
            }
        }

        public int CorrectCount { get; set; }
        public int CorrectTotal { get; set; }
    }
}
