// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.Models.EntityModels
{
    public class PortalRankingModel
    {
        public string? UnitName { get; set; }
        public int TotalStudents { get; set; }
        public int TotalRegisteredStudents { get; set; }
        public int TotalDonePT { get; set; }
        public int TotalActivedCourse { get; set; }
        public double? RateRegisteredOverActual { get; set; }
        public double? RateActiveOverPT { get; set; }
        public double? RateActiveOverRegistered { get; set; }
    }
}
