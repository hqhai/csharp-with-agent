using AutoMapper;
using Fsel.Course.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fsel.Core.Extensions;
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
