using AutoMapper;
using Fsel.Course.Common.Models.Commands.Excercise;
using Fsel.Course.Common.Models.Commands.VideoTimeCode;
using Fsel.Course.Common.Models.Entities;
using Fsel.Course.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fsel.Core.Extensions;

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