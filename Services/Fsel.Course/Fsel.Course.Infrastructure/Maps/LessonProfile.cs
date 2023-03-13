using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Models.CommandModels.Lessons;
using Fsel.Course.Domain.Models.EntiyModels;

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
