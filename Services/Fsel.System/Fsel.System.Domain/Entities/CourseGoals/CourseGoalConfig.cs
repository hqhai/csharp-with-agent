// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities.CourseGoals
{
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using global::System.ComponentModel.DataAnnotations;

    public class CourseGoalConfig : Entity
    {
        [Range(0, int.MaxValue, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int LessonsPerWeek { get; set; }

        public int DisplayOrder { get; set; }
        public Guid CourseId { get; set; }
        public Guid CourseGoalId { get; set; }
        public CourseGoal? CourseGoal { get; set; }
    }
}
