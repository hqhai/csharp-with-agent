// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.Entities.QuestBoards;
    using Fsel.System.Domain.Models.EntityModels;

    public class QuestBoardProfile : Profile
    {
        public QuestBoardProfile()
        {
            CreateMap<QuestBoard, QuestBoardModel>().IgnoreAllNonExisting();
            CreateMap<QuestBoardStudent, QuestBoardStudentModel>().IgnoreAllNonExisting();
        }
    }
}
