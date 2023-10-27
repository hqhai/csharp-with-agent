// Copyright (c) Atlantic. All rights reserved.

using Fsel.Course.Domain.Models.CommandModels.Questions;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Models.CommandModels.Exercises
{
    public class CreateExerciseCommandModel
    {
        public string? Name { get; set; }
        public string? MediaPost { get; set; }
        public EnumCourseSkill CourseSkill { get; set; }
        public IList<CreateQuestionCommandModel>? Questions { get; set; }
    }
}
