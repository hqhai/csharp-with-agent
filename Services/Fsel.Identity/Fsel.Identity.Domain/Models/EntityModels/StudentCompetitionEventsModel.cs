// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class StudentCompetitionEventsModel : BaseModel
    {
        public Guid StudentId { get; set; }

        public Guid CompetitionRankingId { get; set; }

        public CompetitionEventsModel? CompetitionEvents { get; set; }
    }
}
