// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Core.Entities;
    using Fsel.Shared.Models.ShareModels;

    public class CompetitionEvents : Entity
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

        public IList<StudentRankingEvents>? StudentRankingEvents { get; set; }

    }
}
