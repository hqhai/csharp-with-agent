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
            CreateMap<EventTranslation, Event>().IgnoreEntity()?.ReverseMap();
            CreateMap<Event, EventModel>().IgnoreAllNonExisting()?.MapTranslations<Event, EventModel, EventTranslation>();

            CreateMap<PackageEvent, PackageEventModel>().IgnoreAllNonExisting();
            CreateMap<EventTranslation, EventTranslationModel>().IgnoreAllNonExisting();

            CreateMap<SaveEventCommandModel, Event>().IgnoreAllNonExisting();
            CreateMap<SavePackageEventCommandModel, PackageEvent>().IgnoreAllNonExisting();
            CreateMap<SaveEventTranslationCommandModel, EventTranslation>().IgnoreAllNonExisting();
        }
    }
}
