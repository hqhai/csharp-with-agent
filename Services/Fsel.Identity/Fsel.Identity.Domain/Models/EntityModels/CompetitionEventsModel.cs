// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Models.ShareModels;

    public class CompetitionEventsModel : BaseModel
    {
        public string? EventCode { get; set; }

        public string? EventContentStr { get; set; }

        public IList<SchoolEventRule>? EventContent
        {
            get
            {
                return Common.Helpers.ConvertHelper.Deserialize<IList<SchoolEventRule>>(EventContentStr);
            }
            set { EventContentStr = Common.Helpers.ConvertHelper.Serialize(value); }
        }
    }
}
