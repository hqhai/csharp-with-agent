// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;

    public class MockTestAnswerModel : BaseModel
    {
        public string? AnswerStr { get; set; }

        public int CorrectCount { get; set; }

        public SectionQuestion? HomeWorkQuestion { get; set; }

        public MockTestResult? HomeWorkResult { get; set; }

        public Guid SectionQuestionId { get; set; }

        public Guid MockTestResultId { get; set; }
    }
}
