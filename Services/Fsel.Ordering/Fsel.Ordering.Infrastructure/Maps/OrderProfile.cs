// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.Models.CommandModels.Orders;
    using Fsel.Ordering.Domain.Models.EntityModels;

    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<Order, GenerateRamdomOrderModel>().IgnoreAllNonExisting();
            CreateMap<Order, OrderModel>().IgnoreAllNonExisting();
            CreateMap<CreateOrderCommandModel, Order>().IgnoreAllNonExisting();
        }
    }
}
