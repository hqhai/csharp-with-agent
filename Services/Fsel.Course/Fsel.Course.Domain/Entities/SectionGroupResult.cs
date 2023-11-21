// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;

    public class SectionGroupResult : BaseResultScore
    {
        public SectionGroup? SectionGroup { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid SectionGroupId { get; set; }

        public MockTestResult? MockTestResult { get; set; }
        public Guid? MockTestResultId { get; set; }

        public FinalTestResult? FinalTestResult { get; set; }
        public Guid? FinalTestResultId { get; set; }

        public ExtraPracticeResult? ExtraPracticeResult { get; set; }
        public Guid? ExtraPracticeResultId { get; set; }

        public PlacementTestResult? PlacementTestResult { get; set; }
        public Guid? PlacementTestResultId { get; set; }

        public ICollection<PlacementTestAnswer> PlacementTestAnswers { get; set; } = new List<PlacementTestAnswer>();
        public ICollection<FinalTestAnswer> FinalTestAnswers { get; set; } = new List<FinalTestAnswer>();
        public ICollection<ExtraPracticeAnswer> ExtraPracticeAnswers { get; set; } = new List<ExtraPracticeAnswer>();
        public ICollection<MockTestAnswer> MockTestAnswers { get; set; } = new List<MockTestAnswer>();
    }
}
