// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Fsel.Cms.PlanetDefender.Domain.Models.CommandModel;
    using Fsel.Core.Extensions;

    public class WheelOfBuffProfile : Profile
    {
        public WheelOfBuffProfile()
        {
            CreateMap<WheelOfBuffCommandModel, WheelOfBuff>().IgnoreAllNonExisting();
        }
    }
}
