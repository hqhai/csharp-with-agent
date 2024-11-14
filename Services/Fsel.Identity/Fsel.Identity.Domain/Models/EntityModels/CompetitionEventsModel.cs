// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Models.ShareModels;

    public class CompetitionEventsModel : BaseModel
    {
        public string? EventCode { get; set; }

        public string? EventContentStr { get; set; }

        public SchoolEventRule? EventContent
        {
            get
            {
                return Common.Helpers.ConvertHelper.Deserialize<SchoolEventRule>(EventContentStr);
            }
            set { EventContentStr = Common.Helpers.ConvertHelper.Serialize(value); }
        }

        public string? SchoolIdsStr { get; set; }

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
    }
}
