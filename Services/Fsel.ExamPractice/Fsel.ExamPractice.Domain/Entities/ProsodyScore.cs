// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Entities
{
    using Fsel.Core.Entities;

    public class ProsodyScore : Entity
    {
        public double MinScore { get; set; }

        public double MaxScore { get; set; }

        public double BandScore { get; set; }

        public string? BandComment { get; set; }

        public int PoinsInRange
        {
            get
            {
                return (int)(MaxScore - MinScore + 1);
            }
        }
    }
}
