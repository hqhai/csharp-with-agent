// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;

    public class MockTestAnswer : BaseAnswer
    {
        public Guid? SectionQuestionId { get; set; }

        public SectionQuestion? SectionQuestion { get; set; }

        public Guid MockTestResultId { get; set; }
        public MockTestResult? MockTestResult { get; set; }
        public Guid? SectionTimeCodeId { get; set; }
        public SectionTimeCode? SectionTimeCode { get; set; }
        public Guid? SectionId { get; set; }
        public Section? Section { get; set; }
    }
}
