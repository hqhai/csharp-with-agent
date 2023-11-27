// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Fsel.Cms.PlanetDefender.Domain.Models.CommandModel.GameAnswers;
    using Fsel.Cms.PlanetDefender.Domain.Models.CommandModel.GameHistorys;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Core.Extensions;

    public class GameHistoryProfile : Profile
    {
        public GameHistoryProfile()
        {
            CreateMap<GameHistory, GameHistoryModel>().IgnoreAllNonExisting();
            CreateMap<SaveGameHistoryCommandModel, GameHistory>().IgnoreAllNonExisting();
            CreateMap<AnswerTheQuestionCommandModel, GameAnswer>().IgnoreAllNonExisting();
        }
    }
}
