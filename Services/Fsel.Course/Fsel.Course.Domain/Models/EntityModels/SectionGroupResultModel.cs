// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class SectionGroupResultModel : BaseLearnResultModel
    {
        public Guid SectionGroupId { get; set; }
        public Guid? MockTestResultId { get; set; }
        public Guid? FinalTestResultId { get; set; }
        public Guid? ExtraPracticeResultId { get; set; }
        public Guid? PlacementTestResultId { get; set; }
        public bool? IsTestDone { get; set; }
        public double RemainingTime { get; set; }
    }
}
