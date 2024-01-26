// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.BandScoresConfigs
{
    public class BandScoresReport : BandScores
    {
        public double ScoresStudent { get; set; }
        public string? CorrectAnswerStudents { get; set; }
    }
}
