// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.ExtraPracticeAnswers
{
    public class CreateExtraPracticeAnswerMockTestCommandModel
    {
        public Guid ExtraPracticeResultId { get; set; }
        public IList<ExtraPracticeAnswerSectionGroupMTModel>? SectionGroups { get; set; }
        public bool IsActive { get; set; }
    }

    public class ExtraPracticeAnswerSectionGroupMTModel
    {
        public Guid SectionGroupId { get; set; }
        public IList<ExtraPracticeAnswerTypeMockTestModel>? Answers { get; set; }
    }

    public class ExtraPracticeAnswerTypeMockTestModel
    {
        public Guid? SectionId { get; set; }
        public Guid? SectionTimeCodeId { get; set; }
        public Guid? QuestionId { get; set; }
        public object? Answer { get; set; }
    }
}
