using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Course.Common.Models.Commands.Course;
using Fsel.Course.Common.Models.Entities;
using EntityCourse = Fsel.Course.Domain.Entities.Course;

namespace Fsel.Course.Infrastructure.Maps
{
    public class CourseProfile : Profile
    {
        public CourseProfile()
        {
            CreateMap<EntityCourse, CourseModel>().IgnoreAllNonExisting();
            CreateMap<CreateCourseCommandModel, EntityCourse>().IgnoreAllNonExisting();
            CreateMap<UpdateCourseCommandModel, EntityCourse>().IgnoreAllNonExisting();
        }
    }
}