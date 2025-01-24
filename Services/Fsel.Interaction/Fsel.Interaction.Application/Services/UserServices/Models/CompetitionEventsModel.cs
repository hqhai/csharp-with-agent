// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.UserServices.Models
{
    using Fsel.Common.Helpers;
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
                return ConvertHelper.Deserialize<SchoolEventRule>(EventContentStr);
            }
            set { EventContentStr = ConvertHelper.Serialize(value); }
        }
    }
}
