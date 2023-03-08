using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Course.Common.Models.Commands.Course;
using Fsel.Course.Common.Models.Commands.PlacementTest;
using Fsel.Course.Common.Models.Entities;
using Fsel.Course.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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