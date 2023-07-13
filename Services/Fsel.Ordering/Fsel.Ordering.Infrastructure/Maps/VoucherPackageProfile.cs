// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.Models.CommandModels.VoucherPackages;

    public class VoucherPackageProfile : Profile
    {
        public VoucherPackageProfile()
        {
            CreateMap<CreateVoucherPackageModel, VoucherPackage>().IgnoreAllNonExisting();
        }
    }
}
