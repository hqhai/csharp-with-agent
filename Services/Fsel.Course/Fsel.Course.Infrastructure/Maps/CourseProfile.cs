// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Entities.V1i1;
using Fsel.Course.Domain.Models.CommandModels.Courses;
using Fsel.Course.Domain.Models.CommandModels.CourseTeachers;
using Fsel.Course.Domain.Models.EntityModels;
using EntityCourse = Fsel.Course.Domain.Entities.Course;

namespace Fsel.Course.Infrastructure.Maps
{
    public class CourseProfile : Profile
    {
        public CourseProfile()
        {
            CreateMap<EntityCourse, CourseModel>().IgnoreAllNonExisting();
            CreateMap<EntityCourse, EntityCourse>()
                .ForMember(m => m.CourseUnitMockTests, opt => opt.Ignore())
                .ForMember(m => m.Id, opt => opt.Ignore())
                .ForMember(m => m.CourseTeachers, opt => opt.Ignore())
                .ForMember(m => m.CourseResults, opt => opt.Ignore())
                .IgnoreAllNonExisting();

            CreateMap<CreateCourseCommandModel, EntityCourse>().IgnoreAllNonExisting();
            CreateMap<UpdateCourseCommandModel, EntityCourse>().ForMember(m => m.Priority, opt => opt.Ignore()).IgnoreAllNonExisting();
            CreateMap<CourseResult, CourseResultModel>().IgnoreAllNonExisting();
            CreateMap<CourseResult, CourseManagerModel>().IgnoreAllNonExisting();
            CreateMap<EntityCourse, CourseHistoryModel>().IgnoreAllNonExisting();
            CreateMap<EntityCourse, Domain.Models.CommandModels.Courses.V1i1.UpdateCourseCommandModel>().IgnoreAllNonExisting();
            CreateMap<CourseModule, Domain.Models.CommandModels.Courses.V1i1.UpdateCourseModuleModel>().IgnoreAllNonExisting();
            CreateMap<CourseTeacher, CreateCourseTeacherCommandModel>().IgnoreAllNonExisting();
        }
    }
}
