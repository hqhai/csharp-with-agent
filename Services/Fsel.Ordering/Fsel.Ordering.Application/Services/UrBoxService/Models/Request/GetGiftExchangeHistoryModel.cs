// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.UrBoxService.Models.Request
{
    using System;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Refit;

    public class GetGiftExchangeHistoryModel : UrBoxAuthModel
    {
        public GetGiftExchangeHistoryModel(AppSetting appSetting) : base(appSetting)
        {
        }

        [AliasAs("site_user_id")]
        public string? SiteUserId { get; set; }

        [AliasAs("startDate")]
        public DateTime? StartDate { get; set; }

        [AliasAs("endDate")]
        public DateTime? EndDate { get; set; }
    }
}
