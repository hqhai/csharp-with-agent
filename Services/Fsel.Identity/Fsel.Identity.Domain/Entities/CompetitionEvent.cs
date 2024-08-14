// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Core.Entities;
    using Fsel.Shared.Models.ShareModels;

    public class CompetitionEvent : Entity
    {
        public string? EventCode { get; set; }

        public string? EventContentStr { get; set; }

        [NotMapped]
        public SchoolEventRule? EventContent
        {
            get
            {
                return Common.Helpers.ConvertHelper.Deserialize<SchoolEventRule>(EventContentStr);
            }
            set { EventContentStr = Common.Helpers.ConvertHelper.Serialize(value); }
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

        public IList<StudentRankingEvent>? StudentRankingEvents { get; set; }

    }
}
