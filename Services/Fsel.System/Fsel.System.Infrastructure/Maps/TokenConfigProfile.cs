// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Entities.Configs;
    using Fsel.System.Domain.Models.CommandModels.TokenConfigs;

    public class TokenConfigProfile : Profile
    {
        public TokenConfigProfile()
        {
            CreateMap<TokenConfig, TokenConfigModel>().IgnoreAllNonExisting();
            CreateMap<UpdateTokenConfigCommandModel, TokenConfig>().IgnoreAllNonExisting();

            CreateMap<TokenCoinConfigs, TokenConfigDailyCheckIns>();
            //.ForMember(dest => dest.Description, act => act.Ignore())
            //.ForMember(dest => dest.Level, act => act.Ignore());
            CreateMap<TokenCoinConfigs, TokenConfigs>();
            //.ForMember(dest => dest.Description, act => act.Ignore());
            CreateMap<TokenCoinConfigs, TokenFocusModeConfigs>();
            //.ForMember(dest => dest.Description, act => act.Ignore())
            //.ForMember(dest => dest.DisplayOrder, act => act.Ignore())
            //.ForMember(dest => dest.FocusTimeId, act => act.Ignore());
        }
    }
}
