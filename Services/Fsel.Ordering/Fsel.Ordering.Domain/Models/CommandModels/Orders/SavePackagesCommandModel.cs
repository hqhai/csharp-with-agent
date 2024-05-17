// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.Orders
{
    public class SavePackagesCommandModel
    {
        public IList<SavePackageCommandModel>? Packages { get; set; }
    }

    public class SavePackageCommandModel
    {
        public Guid Id { get; set; }
        public decimal Price { get; set; }
    }
}
