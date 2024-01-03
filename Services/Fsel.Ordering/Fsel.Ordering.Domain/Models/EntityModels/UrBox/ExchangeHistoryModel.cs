// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels.UrBox
{
    public class ExchangeHistoryModel
    {
        public IList<DetailGiftExchangeHistory>? UnUsed { get; set; }
        public IList<DetailGiftExchangeHistory>? Used { get; set; }
    }
}
