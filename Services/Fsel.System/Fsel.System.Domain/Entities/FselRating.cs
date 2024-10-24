// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Core.Entities;

    public class FselRating : Entity
    {
        public bool IsRating { get; set; }
        public int Year { get; set; } = DateTime.UtcNow.Year;
        public string? DeviceCode { get; set; }
        public int AmountRating { get; set; }
    }
}
