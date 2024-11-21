// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.Models.CommandModels.Products;
    using Fsel.Ordering.Domain.Models.EntityModels;

    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<SaveProductCommandModel, Product>().IgnoreAllNonExisting();
            CreateMap<SaveProductTranslationCommandModel, ProductTranslation>().IgnoreAllNonExisting();
            CreateMap<ProductTranslation, ProductTranslationModel>().IgnoreAllNonExisting();
            CreateMap<SearchHistoryRedeemByAdminModel, ExportHistoryRedeemProductModel>().IgnoreAllNonExisting();

            CreateMap<ProductTranslation, Product>().IgnoreEntity()?.ReverseMap();
            CreateMap<Product, ProductModel>().IgnoreAllNonExisting()?.MapTranslations<Product, ProductModel, ProductTranslation>();
        }
    }
}
