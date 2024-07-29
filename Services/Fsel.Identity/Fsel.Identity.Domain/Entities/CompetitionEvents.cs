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
        public IList<WeekEvent>? EventContent
        {
            get
            {
                return Common.Helpers.ConvertHelper.Deserialize<IList<WeekEvent>>(EventContentStr);
            }
            set { EventContentStr = Common.Helpers.ConvertHelper.Serialize(value); }
        }

        public StudentRankingEvents? StudentRankingEvents { get; set; }

    }
}
