// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.Orders
{
    public class GetOrderStatusByUserModel
    {
        public Guid? UserId { get; set; }
        public Guid? CourseId { get; set; }
    }
}
