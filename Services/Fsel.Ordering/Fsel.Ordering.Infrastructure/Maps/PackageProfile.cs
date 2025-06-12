// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.Models.CommandModels.Orders;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Ordering.Domain.Models.EntityModels.IntegrationModel;

    public class PackageProfile : Profile
    {
        public PackageProfile()
        {
            CreateMap<Package, PackageModel>().IgnoreAllNonExisting();
            CreateMap<SavePackageCommandModel, Package>().IgnoreAllNonExisting();
            CreateMap<PackageTranslation, Package>().IgnoreEntity()?.ReverseMap();
            CreateMap<PackageTranslation, PackageTranslationModel>().IgnoreAllNonExisting()?.ReverseMap();
            CreateMap<Package, PackageModel>().IgnoreAllNonExisting()?.MapTranslations<Package, PackageModel, PackageTranslation>();

            CreateMap<Package, PackageIntegrationModel>().IgnoreAllNonExisting();
        }
    }
}
