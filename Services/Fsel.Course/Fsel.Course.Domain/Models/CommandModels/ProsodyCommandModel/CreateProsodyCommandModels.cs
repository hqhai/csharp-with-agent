// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.ProsodyCommandModel
{
    using Fsel.Core.Base.BaseModels;

    public class CreateProsodyCommandModels : BaseCommandModel
    {
        public IList<ProsodyCommandModel>? ProsodyModels { get; set; }
    }


    public class ProsodyCommandModel : BaseCommandModel
    {
        public double MinScore { get; set; }
        public double MaxScore { get; set; }

        public double BandScore { get; set; }

        public string? BandComment { get; set; }
    }
}
