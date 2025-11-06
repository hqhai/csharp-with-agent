// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.CourseGoals
{
    using Fsel.Core.Base.BaseModels;
    using global::System.Collections.Generic;

    public class UpdateCourseGoalCommandModel : BaseCommandModel
    {
        public IList<CreateCourseGoalConfigCommandModel> CourseGoalConfigs { get; set; } = new List<CreateCourseGoalConfigCommandModel>();
    }
}
