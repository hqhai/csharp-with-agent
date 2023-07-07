// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.Models.CommandModels.Vouchers;
    using Fsel.Ordering.Domain.Models.EntityModels;

    public class VoucherProfile : Profile
    {
        public VoucherProfile()
        {
            CreateMap<Voucher, VoucherModel>().IgnoreAllNonExisting();
            CreateMap<CreateVoucherCommandModel, Voucher>().IgnoreAllNonExisting();
            CreateMap<UpdateVoucherCommandModel, Voucher>().IgnoreAllNonExisting();
        }
    }
}
