// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.UserServices.Models
{
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
    }
}
