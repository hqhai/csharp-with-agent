// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Models.CommandModels.Units;
using Fsel.Course.Domain.Models.EntityModels;

namespace Fsel.Course.Infrastructure.Maps
{
    public class UnitProfile : Profile
    {
        public UnitProfile()
        {
            CreateMap<Unit, UnitModel>().IgnoreAllNonExisting();
            CreateMap<CreateUnitCommandModel, Unit>().IgnoreAllNonExisting();
            CreateMap<UpdateUnitCommandModel, Unit>().IgnoreAllNonExisting();
            CreateMap<UnitResult, UnitResultModel>().IgnoreAllNonExisting();
        }
    }
}
