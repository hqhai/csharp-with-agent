// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.UrBoxService.Models.Response
{
    public class ExchangeHistoryModel
    {
        public IList<GiftHistoryModel> UnUsed { get; set; } = new List<GiftHistoryModel>();
        public IList<GiftHistoryModel> Used { get; set; } = new List<GiftHistoryModel>();
    }
}
