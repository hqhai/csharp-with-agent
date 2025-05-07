// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.SystemService.QueryModels
{
    public class AddCoinFselEventRewardModel
    {
        public IList<AddCoinFselEventReward>? Values { get; set; }
    }

    public class AddCoinFselEventReward
    {
        public Guid UserId { get; set; }

        public int Coin { get; set; }
    }
}
