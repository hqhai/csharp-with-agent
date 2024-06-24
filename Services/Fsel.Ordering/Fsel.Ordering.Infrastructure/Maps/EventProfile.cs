// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.Models.CommandModels.Events;
    using Fsel.Ordering.Domain.Models.EntityModels;

    public class EventProfile : Profile
    {
        public EventProfile()
        {
            CreateMap<Event, EventModel>().IgnoreAllNonExisting();
            CreateMap<EventTranslation, Event>().IgnoreEntity()?.ReverseMap();
            CreateMap<EventTranslation, EventTranslationModel>().IgnoreAllNonExisting()?.ReverseMap();
            CreateMap<SaveEventCommandModel, Event>().IgnoreAllNonExisting();
            CreateMap<SavePackageEventCommandModel, PackageEvent>().IgnoreAllNonExisting();
            CreateMap<SaveEventTranslationCommandModel, EventTranslation>().IgnoreAllNonExisting();
        }
    }
}
