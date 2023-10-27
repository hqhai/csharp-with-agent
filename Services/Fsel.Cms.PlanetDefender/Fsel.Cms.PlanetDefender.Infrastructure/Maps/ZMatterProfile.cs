// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Fsel.Cms.PlanetDefender.Domain.Models.CommandModel;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Core.Extensions;

    public class ZMatterProfile : Profile
    {
        public ZMatterProfile()
        {
            CreateMap<ZMatter, ZMatterModel>().IgnoreAllNonExisting();
            CreateMap<UpdateStatusZMatterCommandModel, ZMatter>().IgnoreAllNonExisting();
        }
    }
}
