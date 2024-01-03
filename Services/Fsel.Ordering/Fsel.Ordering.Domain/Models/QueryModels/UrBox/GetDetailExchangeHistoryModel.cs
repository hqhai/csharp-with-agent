// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.QueryModels.UrBox
{
    using Refit;

    public class GetDetailExchangeHistoryModel : UrBoxQueryModel
    {
        [AliasAs("transaction_id")]
        public string? TransactionId { get; set; }
    }
}
