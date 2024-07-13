// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.IEntities;

    public class UnitResultModel : BaseScoreResultModel, IModuleLifeCycle
    {
        public double CountQuestion { get; set; }
        public double TotalQuestion { get; set; }
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
        public double? ProgressPercent { get; set; }
        public DateTime? ProcessDate { get; set; }
        public DateTime? CompletionDate { get; set; }
    }
}
