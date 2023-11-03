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
    }
}
