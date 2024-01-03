// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.QueryModels.UrBox
{
    using System;
    using Refit;

    public class GetGiftExchangeHistoryModel : UrBoxQueryModel
    {
        [AliasAs("site_user_id")]
        public string? SiteUserId { get; set; }

        [AliasAs("startDate")]
        public DateTime? StartDate { get; set; }

        [AliasAs("endDate")]
        public DateTime? EndDate { get; set; }
    }
}
