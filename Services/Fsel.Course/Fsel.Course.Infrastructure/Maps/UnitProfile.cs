using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Course.Common.Models.Commands.Unit;
using Fsel.Course.Common.Models.Entities;
using Fsel.Course.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Infrastructure.Maps
{
    public class UnitProfile : Profile
    {
        public UnitProfile()
        {
            CreateMap<Unit, UnitModel>().IgnoreAllNonExisting();
            CreateMap<CreateUnitCommandModel, Unit>().IgnoreAllNonExisting();
            CreateMap<UpdateUnitCommandModel, Unit>().IgnoreAllNonExisting();
        }
    }
}
