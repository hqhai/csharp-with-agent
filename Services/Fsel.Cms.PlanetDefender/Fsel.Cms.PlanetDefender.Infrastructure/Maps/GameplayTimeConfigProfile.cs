// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Fsel.Cms.PlanetDefender.Domain.Models.CommandModel.GameplayTimeConfigs;
    using Fsel.Core.Extensions;

    public class GameplayTimeConfigProfile : Profile
    {
        public GameplayTimeConfigProfile()
        {
            CreateMap<SaveGameplayTimeConfigCommandModel, GameplayTimeConfig>().IgnoreAllNonExisting();
        }
    }
}
