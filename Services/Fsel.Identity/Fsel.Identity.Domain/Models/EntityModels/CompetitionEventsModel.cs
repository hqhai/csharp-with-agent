// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Models.ShareModels;

    public class CompetitionEventsModel : BaseModel
    {
        public string? EventCode { get; set; }
        public string? Name { get; set; }
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

        public string? DashboardEventConfigStr { get; set; }
        public DashboardEventConfig? DashboardEventConfig
        {
            get
            {
                return Common.Helpers.ConvertHelper.Deserialize<DashboardEventConfig>(DashboardEventConfigStr);
            }
            set { DashboardEventConfigStr = Common.Helpers.ConvertHelper.Serialize(value); }
        }

        public Guid? LocationId { get; set; }
        public Guid? ParentEventId { get; set; }
    }
}
