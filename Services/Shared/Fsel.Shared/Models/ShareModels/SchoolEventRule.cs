// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    public class SchoolEventRule
    {
        public string? SchoolCode { get; set; }
        public IList<WeekEvent>? WeekEvents { get; set; }

    }

    public class WeekEvent
    {
        public string? Title { get; set; }

        public IList<WeekRule>? Rules { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int WeekNumber { get; set; }
    }

    public class WeekRule
    {
        public string? PrizeQuantity { get; set; }

        public string? Detail { get; set; }
        public string? RewardImagine { get; set; }
        public int Rank { get; set; }
        public string? Rule { get; set; }
    }
}
