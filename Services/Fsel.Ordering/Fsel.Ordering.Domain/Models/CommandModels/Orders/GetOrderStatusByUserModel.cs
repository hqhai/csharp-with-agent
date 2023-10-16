// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.Orders
{
    public class GetOrderStatusByUserModel
    {
        public Guid? CourseId { get; set; }
        public Guid? ClassId { get; set; }
        public Guid? PackageId { get; set; }
    }
}
