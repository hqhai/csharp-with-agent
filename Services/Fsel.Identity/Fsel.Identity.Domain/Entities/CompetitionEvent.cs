// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;

    public class CompetitionEvent : Entity
    {
        public string? EventCode { get; set; }

        public string? Name { get; set; }

        public string? EventContentStr { get; set; }

        public string? LBConfigStr { get; set; }

        public string? DashboardEventConfigStr { get; set; }

        [NotMapped]
        public SchoolEventRule? LBConfig
        {
            get
            {
                return Common.Helpers.ConvertHelper.Deserialize<SchoolEventRule>(LBConfigStr);
            }
            set { LBConfigStr = Common.Helpers.ConvertHelper.Serialize(value); }
        }

        [NotMapped]
        public SchoolEventRule? EventContent
        {
            get
            {
                return Common.Helpers.ConvertHelper.Deserialize<SchoolEventRule>(EventContentStr);
            }
            set { EventContentStr = Common.Helpers.ConvertHelper.Serialize(value); }
        }

        [NotMapped]
        public DashboardEventConfig? DashboardEventConfig
        {
            get
            {
                return Common.Helpers.ConvertHelper.Deserialize<DashboardEventConfig>(DashboardEventConfigStr);
            }
            set { DashboardEventConfigStr = Common.Helpers.ConvertHelper.Serialize(value); }
        }

        public string? SchoolIdsStr { get; set; }

        [NotMapped]
        public IList<Guid>? SchoolIds
        {
            get
            {
                return Common.Helpers.ConvertHelper.Deserialize<IList<Guid>>(SchoolIdsStr);
            }
            set { SchoolIdsStr = Common.Helpers.ConvertHelper.Serialize(value); }
        }

        public Guid? LocationId { get; set; }

        public Guid? ParentEventId { get; set; }

        public EnumCompetitionEventCategory? Category { get; set; }

        public ICollection<StudentCompetitionEvent> StudentCompetitionEvents { get; set; } = new List<StudentCompetitionEvent>();

        public CompetitionEvent? CompetitionEventParent { get; set; }
        public ICollection<CompetitionEvent> CompetitionEvents { get; set; } = new List<CompetitionEvent>();
    }
}
