// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.CourseTeachers;
    using Fsel.Course.Domain.Models.EntityModels;

    public class CourseTeacherProfile : Profile
    {
        public CourseTeacherProfile()
        {
            CreateMap<CourseTeacher, CourseTeacherModel>().IgnoreAllNonExisting();
            CreateMap<CreateCourseTeacherCommandModel, CourseTeacher>().IgnoreAllNonExisting();
        }
    }
}
