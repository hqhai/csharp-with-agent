// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models;
    using Fsel.Identity.Domain.Models.EntityModels;

    public class EventManagerProfile : Profile
    {
        public EventManagerProfile()
        {
            CreateMap<EventManager, EventManagerModel>().IgnoreAllNonExisting();
            CreateMap<EventManagerModel, EventManager>().IgnoreAllNonExisting();

            CreateMap<CompetitionEvent, CompetitionEventModel>().IgnoreAllNonExisting();
            CreateMap<CompetitionEventModel, CompetitionEvent>().IgnoreAllNonExisting();

            CreateMap<StudentEventLearningRecord, StudentEventLearningRecordModel>().IgnoreAllNonExisting();
        }
    }
}
