// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.V1i1
{
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IEntities;

    public class LessonReportModel : IHighestStreak
    {
        public int CorrectCount { get; set; }
        public int CorrectTotal { get; set; }
        public double Percent { get; set; }
        public double AnswerTime { get; set; }
        public int? HighestStreak { get; set; }
        public int? TimeCodeHighestStreak { get; set; }
        public int TotalToken { get; set; }
        public bool IsShowToken { get; set; }
        public EnumResultStatus? StatusVideoResult { get; set; }
        public EnumBadge? Badge { get; set; }
        public string? BadgeDescription { get; set; }
    }
}
