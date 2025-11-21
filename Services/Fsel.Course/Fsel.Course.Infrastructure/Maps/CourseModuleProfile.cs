// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.ModuleModels;

    public class CourseModuleProfile : Profile
    {
        public CourseModuleProfile()
        {
            CreateMap<CourseModule, CourseModuleModel>().IgnoreAllNonExisting();
            CreateMap<CourseModule, ModuleCourseModel>().IgnoreAllNonExisting();
        }
    }
}
