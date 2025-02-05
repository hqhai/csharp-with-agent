// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using Fsel.Core.Entities;

    public class SchoolImportHistory : Entity
    {
        public Guid SchoolId { get; set; }

        public Guid CompetitionEventId { get; set; }
    }
}
