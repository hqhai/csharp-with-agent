// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.V1i1
{
    public class LessonReportModel
    {
        public double NumberOfCorrect { get; set; }
        public double TotalQuestion { get; set; }
        public double Percent { get; set; }
        public double AnswerTime { get; set; }
    }
}
