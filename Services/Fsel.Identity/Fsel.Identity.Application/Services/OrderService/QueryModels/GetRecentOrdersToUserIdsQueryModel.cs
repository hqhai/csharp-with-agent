// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.OrderService.QueryModels
{
    public class GetRecentOrdersToUserIdsQueryModel
    {
        public IList<Guid>? UserIds { get; set; }
        public DateTime? StartDate { get; set; }
    }
}
