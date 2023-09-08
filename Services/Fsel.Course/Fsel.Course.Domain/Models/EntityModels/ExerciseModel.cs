// Copyright (c) Atlantic. All rights reserved.

using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class ExerciseModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }

        public string? MediaPost { get; set; }

        public EnumCourseSkill CourseSkill { get; set; }

        public IList<QuestionModel>? Questions { get; set; }
    }
}
