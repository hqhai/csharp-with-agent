// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.SystemService.QueryModels
{
    public class AddCoinSurveyRewardModel
    {
        public IList<Guid>? UserIds { get; set; }

        public double Coin { get; set; }
    }
}
