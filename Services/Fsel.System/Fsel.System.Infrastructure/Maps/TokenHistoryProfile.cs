// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Models.CommandModels.TokenHistorys;
    using Fsel.System.Domain.Models.EntityModels;

    public class TokenHistoryProfile : Profile
    {
        public TokenHistoryProfile()
        {
            CreateMap<TokenHistory, TokenHistoryModel>().IgnoreAllNonExisting();
            CreateMap<CreateTokenHistoryCommandModel, TokenHistory>().IgnoreAllNonExisting();
        }
    }
}
