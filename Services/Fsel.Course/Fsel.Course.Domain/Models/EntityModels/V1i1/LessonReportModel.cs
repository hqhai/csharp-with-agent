// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.V1i1
{
    using Fsel.Course.Domain.IEntities;

    public class LessonReportModel : IHighestStreak

    {
        public double NumberOfCorrect { get; set; }
        public double TotalQuestion { get; set; }
        public double Percent { get; set; }
        public double AnswerTime { get; set; }
        public int? HighestStreak { get; set; }
    }
}
