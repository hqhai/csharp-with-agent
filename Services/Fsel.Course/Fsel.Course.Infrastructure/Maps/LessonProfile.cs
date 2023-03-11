using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Course.Common.Models.Commands.Lesson;
using Fsel.Course.Common.Models.Entities;
using Fsel.Course.Domain.Entities;

namespace Fsel.Course.Infrastructure.Maps
{
    public class LessonProfile : Profile
    {
        public LessonProfile()
        {
            CreateMap<Lesson, LessonModel>().IgnoreAllNonExisting();
            CreateMap<CreateLessonCommandModel, Lesson>().IgnoreAllNonExisting();
            CreateMap<UpdateLessonCommandModel, Lesson>().IgnoreAllNonExisting();
        }
    }
}