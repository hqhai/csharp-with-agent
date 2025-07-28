// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using System;
    using Fsel.Core.Entities;

    public class EventManager : Entity
    {
        public Guid UserId { get; set; }
        public virtual User? User { get; set; }

        public Guid CompetitionEventId { get; set; }
        public virtual CompetitionEvent? CompetitionEvent { get; set; }
    }
}
