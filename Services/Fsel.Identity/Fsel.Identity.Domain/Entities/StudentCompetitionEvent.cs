// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using Fsel.Core.Entities;

    public class StudentCompetitionEvent : Entity
    {
        public Guid StudentId { get; set; }

        public Guid CompetitionEventId { get; set; }

        public CompetitionEvent? CompetitionEvents { get; set; }
    }
}
