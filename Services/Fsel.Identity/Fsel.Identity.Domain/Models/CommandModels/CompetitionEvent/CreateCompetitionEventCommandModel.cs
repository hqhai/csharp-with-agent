// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.CompetitionEvent
{
    using Fsel.Shared.Models.ShareModels;

    public class CreateCompetitionEventCommandModel
    {
        public string? EventCode { get; set; }

        public SchoolEventRule? EventContent { get; set; }
        public IList<Guid>? SchoolIds { get; set; }
    }
}
