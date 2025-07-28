// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.TokenHistorys
{
    public class AddCoinFselEventRewardCommandModel
    {
        public Guid UserId { get; set; }

        public int Coin { get; set; }
    }
}
