// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Models.CommandModels.GameTopics;
    using Fsel.System.Domain.Models.EntityModels;

    public class GameTopicConfigProfile : Profile
    {
        public GameTopicConfigProfile()
        {
            CreateMap<GameTopic, GameTopicModel>().IgnoreAllNonExisting();
            CreateMap<SaveGameTopicCommandModel, GameTopic>().IgnoreAllNonExisting();
        }
    }
}
