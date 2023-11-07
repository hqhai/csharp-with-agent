// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;

    public class MockTestAnswer : BaseAnswer
    {
        public SectionQuestion? SectionQuestion { get; set; }
        public Guid? SectionQuestionId { get; set; }
        public SectionTimeCode? SectionTimeCode { get; set; }
        public Guid? SectionTimeCodeId { get; set; }
        public Section? Section { get; set; }
        public Guid? SectionId { get; set; }

        public MockTestResult? MockTestResult { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid MockTestResultId { get; set; }

        public SectionGroupResult? SectionGroupResult { get; set; }
        public Guid? SectionGroupResultId { get; set; }
    }
}
