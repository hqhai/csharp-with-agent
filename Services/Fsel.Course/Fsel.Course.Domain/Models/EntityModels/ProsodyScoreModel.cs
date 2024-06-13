// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class ProsodyScoreModel : BaseModel
    {
        public int PoinsInRange { get; set; }

        public double MinScore { get; set; }

        public double MaxScore { get; set; }

        public double BandScore { get; set; }

        public string? BandComment { get; set; }
    }
}
