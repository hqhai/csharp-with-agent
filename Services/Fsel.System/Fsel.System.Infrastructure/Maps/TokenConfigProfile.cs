// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Models.CommandModels.TokenConfigs;
    using Fsel.System.Domain.Models.EntityModels;

    public class TokenConfigProfile : Profile
    {
        public TokenConfigProfile()
        {
            CreateMap<TokenConfig, TokenConfigModel>().IgnoreAllNonExisting();
            CreateMap<UpdateTokenConfigCommandModel, TokenConfig>().IgnoreAllNonExisting();
        }
    }
}
