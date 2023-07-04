// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class MockTestAnswerModel : BaseModel
    {
        public object? Answer { get; set; }

        public int CorrectCount { get; set; }

        public SectionQuestionModel? HomeWorkQuestion { get; set; }

        public MockTestResultModel? HomeWorkResult { get; set; }

        public Guid SectionQuestionId { get; set; }

        public Guid MockTestResultId { get; set; }
    }
}
