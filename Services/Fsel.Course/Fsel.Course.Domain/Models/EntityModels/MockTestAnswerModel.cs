// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class MockTestAnswerModel : BaseAnswerModel
    {
        public Guid SectionQuestionId { get; set; }
        public Guid MockTestResultId { get; set; }
    }
}
