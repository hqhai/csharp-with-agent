// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Models.CommandModels.CourseTargetConfigs;

    public class CourseTargetConfigProfile : Profile
    {
        public CourseTargetConfigProfile()
        {
            CreateMap<CourseTargetConfig, CourseTargetConfigModel>().IgnoreAllNonExisting();
            CreateMap<CreateCourseTargetConfigCommandModel, CourseTargetConfig>().IgnoreAllNonExisting();
            CreateMap<UpdateCourseTargetConfigCommandModel, CourseTargetConfig>().IgnoreAllNonExisting();
        }
    }
}
