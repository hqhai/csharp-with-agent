// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.UrBoxService.Models.Request
{
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Refit;

    public class GetDetailExchangeHistoryModel : UrBoxAuthModel
    {
        public GetDetailExchangeHistoryModel(AppSetting appSetting) : base(appSetting)
        {
        }

        [AliasAs("transaction_id")]
        public string? TransactionId { get; set; }
    }
}
