// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Fsel.Cms.PlanetDefender.Domain.Models.CommandModel;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Cms.PlanetDefender.Domain.Models.QueryModels.QuestBanks;
    using Fsel.Core.Extensions;

    public class QuestBankProfile : Profile
    {
        public QuestBankProfile()
        {
            CreateMap<QuestBank, QuestBankModel>().IgnoreAllNonExisting();
            CreateMap<CreateQuestBankCommandModel, QuestBank>().IgnoreAllNonExisting();
            CreateMap<SearchQuestBankQueryModel, SearchGameVocabularyQueryModel>().IgnoreAllNonExisting();
        }
    }
}
