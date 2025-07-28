// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using System.Collections.Generic;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;

    public class CompetitionEventModel : BaseModel
    {
        public string? EventCode { get; set; }

        public string? Name { get; set; }

        public string? EventContentStr { get; set; }

        public SchoolEventRule? EventContent { get; set; }

        public Guid? ParentEventId { get; set; }

        public CompetitionEventModel? CompetitionEventParent { get; set; }

        public List<CompetitionEventModel>? CompetitionEvents { get; set; }

        public EnumCompetitionEventCategory? Category { get; set; }
    }
}
