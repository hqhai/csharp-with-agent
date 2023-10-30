// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class UnitResultModel : BaseResultScoreModel
    {
        public double CountQuestion { get; set; }
        public double TotalQuestion { get; set; }
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
    }
}
