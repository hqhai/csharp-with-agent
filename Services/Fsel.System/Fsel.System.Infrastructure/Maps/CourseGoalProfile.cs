// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.Entities.CourseGoals;
    using Fsel.System.Domain.Models.CommandModels.CourseGoals;
    using Fsel.System.Domain.Models.EntityModels;

    public class CourseGoalProfile : Profile
    {
        public CourseGoalProfile()
        {
            CreateMap<CourseGoal, CourseGoalModel>().IgnoreAllNonExisting();
            CreateMap<CourseGoalConfig, CourseGoalConfigModel>().IgnoreAllNonExisting();
            CreateMap<CreateCourseGoalCommandModel, CourseGoal>().IgnoreAllNonExisting();
            CreateMap<CreateCourseGoalConfigCommandModel, CourseGoalConfig>().IgnoreAllNonExisting();
        }
    }
}
