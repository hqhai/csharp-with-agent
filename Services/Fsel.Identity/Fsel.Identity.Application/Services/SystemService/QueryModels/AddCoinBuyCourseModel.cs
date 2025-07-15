// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.SystemService.QueryModels
{
    public class AddCoinBuyCourseModel
    {
        public IList<Guid>? UserIds { get; set; }

        public double Coins { get; set; }

        public int Month { get; set; }
    }
}
