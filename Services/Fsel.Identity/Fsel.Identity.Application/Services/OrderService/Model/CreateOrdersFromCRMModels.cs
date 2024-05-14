// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.OrderService.Model
{
    using Fsel.Shared.Enums;

    public class CreateOrdersFromCRMModels
    {
        public IList<CreateOrdersFromCRMModel>? UsersInfo { get; set; }
    }

    public class CreateOrdersFromCRMModel
    {
        public Guid UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? StudentCode { get; set; }
        public int? DiscountPercent { get; set; }
        public EnumPackageCode PackageCode { get; set; }
    }
}
