// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class StudentCompetitionOverallModel
    {
        public double TotalScore { get; set; }
        public Guid StudentId { get; set; }
    }


    public class StudentCompetitionAverageScore
    {
        public Guid StudentId { get; set; }

        public double AverageScoreByType { get; set; }

        public EnumLearnType LearnType { get; set; }

        public int TotalRecords { get; set; }

        public double LearnRatio { get; set; }
    }
}
