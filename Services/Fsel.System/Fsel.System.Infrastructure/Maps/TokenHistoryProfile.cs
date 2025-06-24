// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Models.EntityModels;

    public class TokenHistoryProfile : Profile
    {
        public TokenHistoryProfile()
        {
            CreateMap<TokenHistory, TokenHistoryModel>().MapTranslations<TokenHistory, TokenHistoryModel, TokenHistoryTranslation>();
            CreateMap<TokenHistoryQueueModel, TokenHistory>().IgnoreAllNonExisting();
            CreateMap<TokenHistoryTranslation, TokenHistory>().IgnoreEntity()?.ReverseMap();
            CreateMap<CreateHistoryDeductCoinOfStudentCommandModel, TokenHistory>().IgnoreAllNonExisting();
            CreateMap<TokenHistoryTranslationModel, TokenHistoryTranslation>().IgnoreAllNonExisting();
        }
    }
}
