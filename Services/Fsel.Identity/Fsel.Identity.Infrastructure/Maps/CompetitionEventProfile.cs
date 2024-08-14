// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.CommandModels.CompetitionEvent;
    using Fsel.Identity.Domain.Models.EntityModels;

    public class CompetitionEventProfile : Profile
    {
        public CompetitionEventProfile()
        {
            CreateMap<CompetitionEvent, CompetitionEventsModel>().IgnoreAllNonExisting();
            CreateMap<CreateCompetitionEventCommandModel, CompetitionEvent>().IgnoreAllNonExisting();
        }
    }
}
