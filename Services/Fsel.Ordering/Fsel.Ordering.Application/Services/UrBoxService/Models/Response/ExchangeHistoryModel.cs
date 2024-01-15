// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.UrBoxService.Models.Response
{
    public class ExchangeHistoryModel
    {
        public IList<DetailGiftExchangeHistory>? UnUsed { get; set; }
        public IList<DetailGiftExchangeHistory>? Used { get; set; }
    }
}
