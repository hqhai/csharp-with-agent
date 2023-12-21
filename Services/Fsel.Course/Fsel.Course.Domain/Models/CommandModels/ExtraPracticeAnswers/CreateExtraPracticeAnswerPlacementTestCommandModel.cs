// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.ExtraPracticeAnswers
{
    public class CreateExtraPracticeAnswerPlacementTestCommandModel
    {
        public Guid ExtraPracticeResultId { get; set; }
        public IList<ExtraPracticeAnswerSectionGroupPTModel>? SectionGroups { get; set; }
        public bool IsSubmit { get; set; }
    }

    public class ExtraPracticeAnswerSectionGroupPTModel
    {
        public Guid SectionGroupId { get; set; }
        public IList<ExtraPracticeAnswerTypePlacementTestModel>? Answers { get; set; }
    }

    public class ExtraPracticeAnswerTypePlacementTestModel
    {
        public Guid QuestionId { get; set; }
        public object? Answer { get; set; }
    }
}
