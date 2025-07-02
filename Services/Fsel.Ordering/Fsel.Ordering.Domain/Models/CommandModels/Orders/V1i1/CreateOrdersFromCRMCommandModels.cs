// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.Orders.V1i1
{
    using Fsel.Shared.Enums;

    public class CreateOrdersFromCRMCommandModels
    {
        public IList<CreateOrdersFromCRMCommandModel>? UsersInfo { get; set; }
    }

    public class CreateOrdersFromCRMCommandModel
    {
        public Guid UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? StudentCode { get; set; }
        public int? DiscountPercent { get; set; }
        public EnumPackageCode PackageCode { get; set; }
    }
}
