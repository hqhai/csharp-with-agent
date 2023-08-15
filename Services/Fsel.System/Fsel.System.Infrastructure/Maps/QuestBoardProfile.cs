// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Models.CommandModels.QuestBoards;
    using Fsel.System.Domain.Models.EntityModels;

    public class QuestBoardProfile : Profile
    {
        public QuestBoardProfile()
        {
            CreateMap<QuestBoard, QuestBoardModel>().IgnoreAllNonExisting();
            CreateMap<QuestBoardTask, QuestBoardTaskModel>().IgnoreAllNonExisting();
            CreateMap<CreateQuestBoardCommandModel, QuestBoard>().IgnoreAllNonExisting();
            CreateMap<UpdateQuestBoardCommandModel, QuestBoard>().IgnoreAllNonExisting();
        }
    }
}
