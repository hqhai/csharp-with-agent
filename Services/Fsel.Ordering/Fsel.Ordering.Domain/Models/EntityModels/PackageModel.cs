// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels
{
    public class PackageModel
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public double Price { get; set; }
        public IList<string>? Description { get; set; }
    }
}
