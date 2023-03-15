// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.CourseUnitMockTests;

    public class CourseUnitMockTestProfile : Profile
    {
        public CourseUnitMockTestProfile()
        {
            CreateMap<CreateCourseUnitMockTestCommandModel, CourseUnitMockTest>().IgnoreAllNonExisting();
            CreateMap<UpdateCourseUnitMockTestCommandModel, CourseUnitMockTest>().IgnoreAllNonExisting();
        }
    }
}
