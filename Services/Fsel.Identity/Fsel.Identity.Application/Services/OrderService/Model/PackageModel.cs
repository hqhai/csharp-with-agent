// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.OrderService.Model
{
    public class PackageModel
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public double Price { get; set; }
        public int MonthNumber { get; set; }
    }
}
