// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class EventManagerModel : BaseModel
    {
        public Guid UserId { get; set; }
        public Guid CompetitionEventId { get; set; }

        public Guid LocationId { get; set; }
    }
}
