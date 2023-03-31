// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Enums;
using Fsel.Course.Domain.Models.CommandModels.Questions;

namespace Fsel.Course.Domain.Models.CommandModels.Exercises
{
    public class UpdateExerciseCommandModel
    {
        public string? Name { get; set; }

        public string? MediaPost { get; set; }

        public EnumCourseSkill CourseSkill { get; set; }

        public List<UpdateQuestionCommandModel> Questions { get; set; } = new List<UpdateQuestionCommandModel>();
    }
}
