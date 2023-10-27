// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.Models.CommandModels.VoucherPackages;
    using Fsel.Ordering.Domain.Models.EntityModels;

    public class VoucherPackageProfile : Profile
    {
        public VoucherPackageProfile()
        {
            CreateMap<VoucherPackage, VoucherPackageModel>().IgnoreAllNonExisting();
            CreateMap<CreateVoucherPackageModel, VoucherPackage>().IgnoreAllNonExisting();
        }
    }
}
