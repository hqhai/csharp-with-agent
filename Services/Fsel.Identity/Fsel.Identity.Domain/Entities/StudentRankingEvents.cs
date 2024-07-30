// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using Fsel.Core.Entities;

    public class StudentRankingEvents : Entity
    {
        public Guid StudentId { get; set; }

        public Guid CompetitionRankingId { get; set; }

        public CompetitionEvents? CompetitionEvents { get; set; }
    }
}
