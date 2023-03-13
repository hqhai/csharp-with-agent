using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Models.CommandModels.Excercises;

namespace Fsel.Course.Infrastructure.Maps
{
    public class ExcerciseProfile : Profile
    {
        public ExcerciseProfile()
        {
            //CreateMap<VideoTimeCode, VideoTimeCodeModel>().IgnoreAllNonExisting();
            CreateMap<CreateExcerciseCommandModel, Excercise>().IgnoreAllNonExisting();
            CreateMap<UpdateExcerciseCommandModel, Excercise>().IgnoreAllNonExisting();
        }
    }
}
