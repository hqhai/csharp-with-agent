// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.PlacementTestAnswers
{
    using Fsel.Shared.Enums;

    public class CreatePlacementTestAnswerCommandModel
    {
        public EnumPlacementTestLevel Level { get; set; }
        public double TotalQuestion { get; set; }
        public double CountQuestion { get; set; }
        public IList<PlacementTestAnswerSkillQuestionModel>? Skills { get; set; }
    }

    public class PlacementTestAnswerQuestionModel
    {
        public Guid QuestionId { get; set; }
        public object? Answer { get; set; }
    }

    public class PlacementTestAnswerSkillQuestionModel
    {
        public EnumCourseSkill Skill { get; set; }
        public IList<PlacementTestAnswerQuestionModel>? Answers { get; set; }
    }
}
