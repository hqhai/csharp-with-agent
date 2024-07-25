// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class StudentCompetitionSnapShotModel : BaseModel
    {
        public string? SchoolCode { get; set; }

        public string? WeekCompetitionData { get; set; }

        public int WeekNumber { get; set; }

        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }
    }
}
