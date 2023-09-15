// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.FinalTestAnswers
{
    using Fsel.Shared.Enums;

    public class CreateFinalTestAnswerCommandModel
    {
        public Guid CourseId { get; set; }
        public Guid FinalTestId { get; set; }
        public long AccessTime { get; set; }
        public IList<FinalTestAnswerSkillQuestionModel>? FinalTestAnswers { get; set; }
    }

    public class FinalTestAnswerQuestionModel
    {
        public Guid QuestionId { get; set; }
        public object? Answer { get; set; }
    }

    public class FinalTestAnswerSkillQuestionModel
    {
        public EnumCourseSkill Skill { get; set; }
        public IList<FinalTestAnswerQuestionModel>? Answers { get; set; }
    }
}
