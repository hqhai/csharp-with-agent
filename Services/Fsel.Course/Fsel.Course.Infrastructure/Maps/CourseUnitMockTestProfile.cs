// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.CourseUnitMockTests;
    using Fsel.Course.Domain.Models.EntityModels;

    public class CourseUnitMockTestProfile : Profile
    {
        public CourseUnitMockTestProfile()
        {
            CreateMap<CreateCourseUnitMockTestCommandModel, CourseUnitMockTest>().IgnoreAllNonExisting();
            CreateMap<UpdateCourseUnitMockTestCommandModel, CourseUnitMockTest>().IgnoreAllNonExisting();
            CreateMap<CourseUnitMockTest, CourseUnitMockTestModel>()
            .ForMember(dest => dest.Type, opt =>
                opt.MapFrom(src =>
                    src.FinalTestId.HasValue ? nameof(src.FinalTest) :
                    src.MockTestId.HasValue ? nameof(src.MockTest) :
                    src.UnitId.HasValue ? nameof(src.Unit) : null));
        }
    }
}
