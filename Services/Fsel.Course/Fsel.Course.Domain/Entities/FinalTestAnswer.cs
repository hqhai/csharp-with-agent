// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;

    public class FinalTestAnswer : BaseAnswer
    {
        public FinalTestResult? FinalTestResult { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid FinalTestResultId { get; set; }

        public SectionQuestion? SectionQuestion { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid SectionQuestionId { get; set; }

        public SectionGroupResult? SectionGroupResult { get; set; }
        public Guid? SectionGroupResultId { get; set; }
    }
}
