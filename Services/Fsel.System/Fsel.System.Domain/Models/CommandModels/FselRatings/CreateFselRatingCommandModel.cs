// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.FselRatings
{
    public class CreateFselRatingCommandModel
    {
        public string? DeviceCode { get; set; }
        public bool IsRating { get; set; }
    }
}
