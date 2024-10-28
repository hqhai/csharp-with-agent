// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class FselRatingModel : BaseModel
    {
        public bool IsRating { get; set; }
        public int Year { get; set; } = DateTime.UtcNow.Year;
        public int AmountRating { get; set; }
    }
}
