// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.Models.CommandModels.Orders.V1i1;
    using Fsel.Ordering.Domain.Models.CommandModels.Orders.V1i2;
    using Fsel.Ordering.Domain.Models.EntityModels;

    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<Order, OrderModel>().IgnoreAllNonExisting();
            CreateMap<Domain.Models.CommandModels.Orders.CreateOrderCommandModel, Order>().IgnoreAllNonExisting();
            CreateMap<Domain.Models.CommandModels.Orders.V1i1.CreateOrderCommandModel, Order>().IgnoreAllNonExisting();
            CreateMap<Domain.Models.CommandModels.Orders.V1i2.CreateOrderCommandModel, Order>().IgnoreAllNonExisting();
            CreateMap<Domain.Models.CommandModels.Orders.V1i2.CreateOrderByUserIdCommandModel, Order>().IgnoreAllNonExisting();

            CreateMap<CreateOrderToUserIdCommandModel, Order>().IgnoreAllNonExisting();
            CreateMap<Order, OrderSearchModel>().ForMember(x => x.PackageName, a => a.MapFrom(src => src.Package != null ? src.Package.Code : null))
                                                .ForMember(x => x.MonthNumber, a => a.MapFrom(src => src.Package!.MonthNumber));
            CreateMap<CreateOrderPaymentCommandModel, Order>().IgnoreAllNonExisting();
        }
    }
}
