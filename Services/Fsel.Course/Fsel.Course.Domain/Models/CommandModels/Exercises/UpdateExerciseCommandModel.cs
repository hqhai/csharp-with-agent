// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Exercises
{
    using Fsel.Course.Domain.Models.CommandModels.Questions;
    using Fsel.Shared.Enums;

    public class UpdateExerciseCommandModel
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public string? MediaPost { get; set; }
        public EnumCourseSkill CourseSkill { get; set; }
        public Guid? SkillId { get; set; }
        public IList<UpdateQuestionCommandModel>? Questions { get; set; }
    }
}
