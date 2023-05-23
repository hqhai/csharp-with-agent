// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.FinalTestExerciseAnswers
{
    using Fsel.Shared.Enums;

    public class CreateFinalTestExerciseAnswerCommandModel
    {
        public Guid CourseId { get; set; }
        public Guid FinalTestId { get; set; }
        public IList<FinalTestExerciseAnswerSkillQuestionModel>? Skills { get; set; }
    }

    public class FinalTestExerciseAnswerQuestionModel
    {
        public Guid QuestionId { get; set; }
        public object? Answer { get; set; }
    }
    public class FinalTestExerciseAnswerSkillQuestionModel
    {
        public EnumCourseSkill Skill { get; set; }
        public IList<FinalTestExerciseAnswerQuestionModel>? Answers { get; set; }
    }
}
