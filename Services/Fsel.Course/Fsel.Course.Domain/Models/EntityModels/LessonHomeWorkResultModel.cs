// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class LessonHomeWorkResultModel : HomeWorkModel
    {
        public Guid? LessonModuleId { get; set; }
        public long QuestionCompleted { get; set; }
        public long QuestionTotal { get; set; }

        public double QuestionPercent
        { get { return QuestionTotal > 0 ? (QuestionCompleted / (double)QuestionTotal) * 100 : default; } }

        public int CorrectCount { get; set; }
        public int CorrectTotal { get; set; }
    }
}
