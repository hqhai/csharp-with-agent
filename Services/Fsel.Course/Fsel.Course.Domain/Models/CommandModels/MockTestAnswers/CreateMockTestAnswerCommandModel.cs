// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.MockTestAnswers
{
    using System;
    using System.Collections.Generic;

    public class CreateMockTestAnswerCommandModel
    {
        public Guid MockTestResultId { get; set; }
        public IList<MockTestSectionGroupAnswerModel>? SectionGroups { get; set; }
    }

    public class MockTestAnswerQuestionModel
    {
        public Guid? SectionTimeCodeId { get; set; }
        public Guid? SectionId { get; set; }
        public Guid? QuestionId { get; set; }
        public object? Answer { get; set; }
    }

    public class MockTestSectionGroupAnswerModel
    {
        public Guid SectionGroupId { get; set; }
        public IList<MockTestAnswerQuestionModel>? Answers { get; set; }
    }
}
