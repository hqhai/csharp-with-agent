// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.Products
{
    using Fsel.Ordering.Domain.Entities;

    public class SaveProductCommandModel
    {
        public Guid? Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public int Quantity { get; set; }
        public bool ShowPriority { get; set; }
        public int Price { get; set; }
        public ProductDescription? Description { get; set; }
        public DateTime ExpireDate { get; set; }
        public IList<string>? Images { get; set; }
        public IList<Guid>? EventIds { get; set; }
        public IList<SaveProductTranslationCommandModel>? Translations { get; set; }
    }

    public class SaveProductTranslationCommandModel
    {
        public string? Name { get; set; }
        public ProductDescription? Description { get; set; }
        public string? Language { get; set; }
    }
}
