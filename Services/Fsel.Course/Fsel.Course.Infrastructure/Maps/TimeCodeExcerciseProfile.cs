using AutoMapper;
using Fsel.Course.Domain.Entities;
using System;
using Fsel.Core.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Infrastructure.Maps
{
    public class TimeCodeExcerciseProfile : Profile
    {
        public TimeCodeExcerciseProfile()
        {
            //CreateMap<TimeCodeExcercise, TimeCodeExcercise>().IgnoreAllNonExisting();
            //CreateMap<CreateTimeCodeExcerciseCommandModel, TimeCodeExcercise>().IgnoreAllNonExisting();
            //CreateMap<UpdateTimeCodeExcerciseCommandModel, TimeCodeExcercise>().IgnoreAllNonExisting();
        }
    }
}
