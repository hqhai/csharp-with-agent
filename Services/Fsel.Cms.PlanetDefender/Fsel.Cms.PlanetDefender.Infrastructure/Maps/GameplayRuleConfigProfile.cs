// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Fsel.Cms.PlanetDefender.Domain.Models.CommandModel.GameplayRuleConfigs;
    using Fsel.Cms.PlanetDefender.Domain.Models.QueryModels.GameplayRuleConfigs;
    using Fsel.Core.Extensions;

    public class GameplayRuleConfigProfile : Profile
    {
        public GameplayRuleConfigProfile()
        {
            CreateMap<SaveGameplayRuleConfigCommandModel, GameplayRuleConfig>().IgnoreAllNonExisting();
            CreateMap<GameplayRuleConfig, GameplayRuleConfigsModel>().IgnoreAllNonExisting();
        }
    }
}
