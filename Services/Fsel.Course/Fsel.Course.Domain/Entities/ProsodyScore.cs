// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Core.Entities;

    public class ProsodyScore : Entity
    {
        public int PoinsInRange { get; }

        public double MinScore { get; set; }

        public double MaxScore { get; set; }

        public double BandScore { get; set; }

        public string? BandComment { get; set; }
    }
}
