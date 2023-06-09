// Copyright (c) Atlantic. All rights reserved.

using Fsel.Shared.Enums;
using Fsel.Course.Domain.Models.CommandModels.Questions;
using System.ComponentModel.DataAnnotations;

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
