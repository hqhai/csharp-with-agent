// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Shared.Enums;

    public class ProductModel : BaseModel
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public int Quantity { get; set; }
        public bool ShowPriority { get; set; }
        public int Price { get; set; }
        public int QuantityChanged { get; set; }
        public int RemainingQuantity { get; set; }
        public bool ProductStatus { get; set; }
        public EnumMarketPlaceType MarketPlaceType { get; set; }
        public EnumProductType? ProductType { get; set; }
        public EnumProductStatus Status { get; set; }
        public DateTime ExpireDate { get; set; }
        public IList<string>? Images { get; set; }
        public IList<Guid>? EventIds { get; set; }
        public string? GlobalId { get; set; }
        public bool IsPremium { get; set; }
        public string? BrandName { get; set; }
        public string? BrandImage { get; set; }
        public ProductDescription? Description { get; set; }
        public ProductGlobalConfig? ProductGlobalConfig { get; set; }
        public IList<ProductTranslationModel>? Translations { get; set; }
    }

    public class ProductTranslationModel : BaseModel
    {
        public string? Name { get; set; }
        public ProductDescription? Description { get; set; }
        public string? Language { get; set; }
    }
}
