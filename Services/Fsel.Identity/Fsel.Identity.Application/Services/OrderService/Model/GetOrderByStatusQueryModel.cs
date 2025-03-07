// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.OrderService.Model
{
    public class GetOrderByStatusQueryModel
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool Status { get; set; }

        public IList<Guid>? UserIds { get; set; }
    }
}
