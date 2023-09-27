// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Models.CommandModels.GameVocabularies;
    using Fsel.System.Domain.Models.EntityModels;

    public class GameVocabularyProfile : Profile
    {
        public GameVocabularyProfile()
        {
            CreateMap<CreateGameVocabularyCommandModel, GameVocabulary>().IgnoreAllNonExisting();
            CreateMap<UpdateGameVocabularyCommandModel, GameVocabulary>().IgnoreAllNonExisting();
            CreateMap<GameVocabulary, GameVocabularyModel>().IgnoreAllNonExisting();
        }
    }
}
