// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Fsel.Cms.PlanetDefender.Domain.Models.CommandModel.Events;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Core.Extensions;

    public class EventProfile : Profile
    {
        public EventProfile()
        {
            CreateMap<Event, EventModel>().IgnoreAllNonExisting();
            CreateMap<CreateEventCommandModel, Event>().IgnoreAllNonExisting();
            CreateMap<UpdateEventCommandModel, Event>().IgnoreAllNonExisting();
        }
    }
}
